# Οριστικό πρωτόκολλο για 12 datasets

## Baseline εκτέλεση

Για κάθε πρόταση χρησιμοποίησε ένα σταθερό Dataset ID, π.χ. `dataset-01`. Τα επαναλαμβανόμενα runs του ίδιου dataset παίρνουν αυτόματα διαφορετικό Run ID.

1. Επίλεξε το σωστό evaluation profile.
2. Πρόσθεσε Evaluation Context και Proposal.
3. Εκτέλεσε την Independent Evaluation.
4. Πρόσθεσε ESR και καταχώρισε τα επίσημα scores ανά κριτήριο.
5. Εκτέλεσε το ESR Comparison.
6. Αποθήκευσε το Experiment Run και κατέβασε το JSON.

Γίνονται τρεις ανεξάρτητες επαναλήψεις ανά dataset. Για τα 12 datasets αυτό αντιστοιχεί σε 36 baseline runs και επιτρέπει να μετρηθεί η μεταβλητότητα του μοντέλου.

## Σταθερές συνθήκες

Κατά το baseline πείραμα κράτησε σταθερά:

- το ακριβές model snapshot,
- το reasoning effort,
- το maximum output token limit,
- το σταθερό model/reasoning/token limit της ποιοτικής ESR σύγκρισης,
- το prompt template,
- το evaluation profile,
- τα Evaluation Context, Proposal και ESR αρχεία,
- τα επίσημα ESR scores.

Αλλαγή prompt, model ή scoring profile αποτελεί νέα πειραματική συνθήκη και δεν πρέπει να αναμιγνύεται με τα baseline runs.

## Sensitivity experiment για model και reasoning effort

Η κύρια ανάλυση παραμένει το baseline των 36 runs με:

- evaluation model `gpt-5.4-mini-2026-03-17`,
- evaluation reasoning effort `medium`,
- maximum output tokens `5000`.

Η sensitivity analysis περιορίζεται σε τρία ετερογενή datasets: CRADLE, INSPIRE 2025 και ODE4Change. Για καθένα γίνονται τρεις επαναλήψεις στις παρακάτω συνθήκες:

| Συνθήκη | Evaluation model | Reasoning | Χρήση |
|---|---|---|---|
| A | `gpt-5.4-mini-2026-03-17` | `medium` | Επαναχρησιμοποίηση 9 baseline runs |
| B | `gpt-5.4-mini-2026-03-17` | `high` | 9 πρόσθετα runs |
| C | `gpt-5.4-2026-03-05` | `medium` | 9 πρόσθετα runs |

Η ποιοτική ESR σύγκριση παραμένει σε όλες τις συνθήκες στο `gpt-5.4-mini-2026-03-17`, reasoning `medium` και `5000` maximum output tokens. Συνεπώς εκτελούνται 36 κύριες και 18 πρόσθετες αξιολογήσεις, δηλαδή 54 συνολικά runs, χωρίς να θεωρούνται τα repeated runs ανεξάρτητα datasets.

## Έλεγχος πριν από την ανάλυση

Για κάθε run επιβεβαίωσε ότι:

- δεν υπάρχουν errors,
- το schema version είναι 2.1,
- τα document hashes είναι ίδια μεταξύ επαναλήψεων του ίδιου dataset,
- το prompt hash και το configured model είναι ίδια σε ολόκληρη την ίδια πειραματική συνθήκη,
- υπάρχει ESR comparison όταν το dataset διαθέτει ESR.

Οι συνθήκες αλλάζουν χωρίς αλλαγή κώδικα από το section `OpenAI` του `appsettings.json`
ή από τις αντίστοιχες environment variables (`OpenAI__Model`, `OpenAI__ReasoningEffort`,
`OpenAI__MaxOutputTokens`). Τα `OpenAI__ComparisonModel`,
`OpenAI__ComparisonReasoningEffort` και `OpenAI__ComparisonMaxOutputTokens` παραμένουν
σταθερά στο baseline κατά το model/reasoning sensitivity experiment. Κάθε αποθηκευμένο
run κρατά ξεχωριστό snapshot και για τα δύο requests.

## Ελάχιστες μετρικές

Από τα CSV exports μπορούν να υπολογιστούν:

- mean absolute error ανά κριτήριο,
- mean signed error για έλεγχο συστηματικής υπερ- ή υπο-βαθμολόγησης,
- mean absolute error συνολικού score,
- threshold agreement rate,
- διακύμανση των τριών LLM runs ανά dataset,
- πλήθος shared findings, findings only by LLM και findings only in ESR.

Η ποσοτική ανάλυση πρέπει να συνοδεύεται από ποιοτική εξέταση των false agreements: περιπτώσεις όπου υπάρχει threshold agreement αλλά μεγάλη αριθμητική ή ουσιαστική απόκλιση.
