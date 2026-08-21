# Μεθοδολογία υπολογισμού βαθμολογίας

## Διαχωρισμός LLM και C#

Το γλωσσικό μοντέλο δεν αποφασίζει το συνολικό σκορ ούτε το αποτέλεσμα του threshold. Επιστρέφει, μέσω Structured Output, μόνο:

- την ποιοτική αξιολόγηση,
- ένα raw score για κάθε αναμενόμενο κριτήριο,
- τεκμήρια, strengths, shortcomings και limitations.

Η εφαρμογή εκτελεί έπειτα ντετερμινιστικά στη C# τα παρακάτω βήματα:

1. Ελέγχει ότι υπάρχει ακριβώς ένα score για κάθε κριτήριο του ενεργού profile.
2. Απορρίπτει άγνωστα, ελλιπή ή διπλά criterion IDs.
3. Ελέγχει το επιτρεπτό εύρος και, όπου υπάρχει, το βήμα βαθμολογίας.
4. Υπολογίζει τη συνεισφορά κάθε κριτηρίου στο συνολικό σκορ.
5. Αθροίζει τις συνεισφορές.
6. Εφαρμόζει τα επιμέρους και το συνολικό threshold.

Η κεντρική υλοποίηση βρίσκεται στο `Services/ScoreCalculator.cs`. Το `Services/EvaluationResultProcessor.cs` εφαρμόζει τον υπολογισμό στην ανεξάρτητη αξιολόγηση και το `Services/ComparisonCalculator.cs` εφαρμόζει ακριβώς τους ίδιους κανόνες στα LLM και ESR scores.

## Additive scoring

Στο additive mode η συνεισφορά ενός κριτηρίου ισούται με το raw score:

\[
C_i = s_i
\]

και το συνολικό σκορ είναι:

\[
S = \sum_{i=1}^{n}s_i
\]

Για τα τυπικά Horizon profiles της εφαρμογής:

- κάθε score είναι από 0 έως 5 με βήμα 0,5,
- κάθε criterion threshold είναι 3,
- το overall threshold είναι 10 στα 15,
- επιτυχία υπάρχει μόνο όταν ικανοποιούνται και τα τρία επιμέρους thresholds και το συνολικό threshold.

Παράδειγμα:

\[
S = 4.0 + 4.0 + 4.0 = 12.0
\]

Το 12/15 περνά το overall threshold και, επειδή κάθε score είναι τουλάχιστον 3, το αποτέλεσμα είναι `Threshold met`.

Αν τα scores είναι 3,0, 2,5 και 3,0, τότε:

\[
S = 8.5
\]

Το αποτέλεσμα αποτυγχάνει τόσο στο overall threshold όσο και στο επιμέρους threshold του δεύτερου κριτηρίου.

## Weighted percentage scoring

Στο weighted mode κάθε raw score κανονικοποιείται ως προς το μέγιστο score του κριτηρίου και πολλαπλασιάζεται με το βάρος του:

\[
C_i = \frac{s_i}{s_{i,max}} \times w_i
\]

Το συνολικό weighted score είναι:

\[
S = \sum_{i=1}^{n}C_i
\]

Για το MSCA Staff Exchanges profile της εφαρμογής τα βάρη είναι 50%, 30% και 20%, με μέγιστο raw score 5. Για scores 3,4, 3,7 και 3,8:

\[
C_1 = \frac{3.4}{5}\times50 = 34
\]

\[
C_2 = \frac{3.7}{5}\times30 = 22.2
\]

\[
C_3 = \frac{3.8}{5}\times20 = 15.2
\]

άρα:

\[
S = 34 + 22.2 + 15.2 = 71.4
\]

Με overall threshold 70/100, το αποτέλεσμα περνά. Στο τρέχον MSCA profile δεν έχουν οριστεί επιμέρους criterion thresholds.

## Erasmus+ profile

Το Erasmus+ CBHE Strand 2 profile χρησιμοποιεί additive scoring με μέγιστα 30, 30, 20 και 20. Το συνολικό threshold είναι 60/100 και τα επιμέρους thresholds είναι αντίστοιχα 15, 15, 10 και 10.

Επομένως, ακόμη και αν το άθροισμα είναι τουλάχιστον 60, η αξιολόγηση δεν περνά όταν έστω ένα κριτήριο βρίσκεται κάτω από το δικό του threshold.

## Σύγκριση με ESR

Για κάθε κριτήριο η αριθμητική διαφορά ορίζεται ως:

\[
\Delta_i = s_{i,LLM} - s_{i,ESR}
\]

και η διαφορά συνολικού σκορ ως:

\[
\Delta_S = S_{LLM} - S_{ESR}
\]

Το `ThresholdAgreement` είναι `true` μόνο όταν LLM και ESR καταλήγουν στην ίδια δυαδική απόφαση επιτυχίας ή αποτυχίας. Δεν σημαίνει ότι έχουν ίσα scores.

## Τι αποθηκεύεται για έλεγχο

Κάθε νέο experiment run schema 2.0 αποθηκεύει:

- το πλήρες snapshot του evaluation profile και των κανόνων scoring,
- το prompt template και το SHA-256 του,
- το configured model, reasoning effort και token limit,
- raw scores, score breakdown, totals και threshold decisions,
- τα αποτελέσματα σύγκρισης και τα API usage metadata,
- hashes και character counts των εγγράφων.

Το source text των εγγράφων και το πλήρες generated prompt εξαιρούνται από το downloadable JSON, ώστε το αρχείο να μπορεί να χρησιμοποιηθεί για ανάλυση χωρίς να διαμοιράζει το περιεχόμενο της πρότασης.
