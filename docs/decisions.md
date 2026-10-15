# Decisions – Digital Wardrobe

This document records the important technical decisions made in the project, together with the reasons for each decision. New decisions are added to the end of the table with their date, and a decision that is later changed is not deleted but marked as replaced by the newer entry, so that the history of the project remains visible.

| # | Date | Decision | Reason |
|---|---|---|---|
| 1 | 2026-10-14 | Tags are stored in a separate table with a join table between items and tags. | Tags are created by users, can be renamed and are shared between many items. |
| 2 | 2026-10-14 | Seasons are stored as an array of enum values. | Seasons are a fixed list defined in the code, so a join table would add tables without benefit. |
| 3 | 2026-10-14 | Colours are free text stored as a text array, trimmed and in lowercase. | Users can describe colours freely. Different spellings and the lack of renaming across items are accepted as limitations. |
| 4 | 2026-10-14 | The occasion field from the original brief was removed. | The number of possible occasions is too large for a fixed list, and users can describe occasions with tags. |
| 5 | 2026-10-14 | Each item has at most one photo, referenced by a server-generated key. | One photo is enough for the first version and keeps the data model simple. |
| 6 | 2026-10-14 | The solution is divided into three projects instead of one. | To learn how to separate layers and control dependencies, with the compiler enforcing the rules. The number of projects is kept small on purpose. |
| 7 | 2026-10-14 | Services are placed in `Core` and use repository interfaces implemented in `Data`. | To keep the rules of the application separate from database code and to allow services to be unit tested without a database. |
| 8 | 2026-10-14 | Every repository method requires the user ID. | Data isolation is built into the interface, so a forgotten ownership check cannot happen when new methods are added. |
| 9 | 2026-10-14 | In development, only PostgreSQL runs in Docker. | The application can be debugged and restarted quickly, while the full Docker setup is still used for production. |
