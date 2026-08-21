# Προτεινόμενο πρωτόκολλο για 12–16 datasets

## Baseline εκτέλεση

Για κάθε πρόταση χρησιμοποίησε ένα σταθερό Dataset ID, π.χ. `dataset-01`. Τα επαναλαμβανόμενα runs του ίδιου dataset παίρνουν αυτόματα διαφορετικό Run ID.

1. Επίλεξε το σωστό evaluation profile.
2. Πρόσθεσε Evaluation Context και Proposal.
3. Εκτέλεσε την Independent Evaluation.
4. Πρόσθεσε ESR και καταχώρισε τα επίσημα scores ανά κριτήριο.
5. Εκτέλεσε το ESR Comparison.
6. Αποθήκευσε το Experiment Run και κατέβασε το JSON.

Προτείνεται να γίνουν τρεις ανεξάρτητες επαναλήψεις ανά dataset. Για 12–16 datasets αυτό αντιστοιχεί σε 36–48 runs και επιτρέπει να μετρηθεί η μεταβλητότητα του μοντέλου.

## Σταθερές συνθήκες

Κατά το baseline πείραμα κράτησε σταθερά:

- το ακριβές model snapshot,
- το reasoning effort,
- το prompt template,
- το evaluation profile,
- τα Evaluation Context, Proposal και ESR αρχεία,
- τα επίσημα ESR scores.

Αλλαγή prompt, model ή scoring profile αποτελεί νέα πειραματική συνθήκη και δεν πρέπει να αναμιγνύεται με τα baseline runs.

## Έλεγχος πριν από την ανάλυση

Για κάθε run επιβεβαίωσε ότι:

- δεν υπάρχουν errors,
- το schema version είναι 2.0,
- τα document hashes είναι ίδια μεταξύ επαναλήψεων του ίδιου dataset,
- το prompt hash και το configured model είναι ίδια σε ολόκληρη την ίδια πειραματική συνθήκη,
- υπάρχει ESR comparison όταν το dataset διαθέτει ESR.

## Ελάχιστες μετρικές

Από τα CSV exports μπορούν να υπολογιστούν:

- mean absolute error ανά κριτήριο,
- mean signed error για έλεγχο συστηματικής υπερ- ή υπο-βαθμολόγησης,
- mean absolute error συνολικού score,
- threshold agreement rate,
- διακύμανση των τριών LLM runs ανά dataset,
- πλήθος shared findings, findings only by LLM και findings only in ESR.

Η ποσοτική ανάλυση πρέπει να συνοδεύεται από ποιοτική εξέταση των false agreements: περιπτώσεις όπου υπάρχει threshold agreement αλλά μεγάλη αριθμητική ή ουσιαστική απόκλιση.
