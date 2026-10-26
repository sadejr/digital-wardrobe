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
| 10 | 2026-10-26 | The empty first migration is kept. | It was created to check the database connection. It causes no problems, and removing it would not remove it from the Git history. |
| 11 | 2026-10-26 | The database has a healthcheck, and the application container waits until it is healthy. | `depends_on` alone only waits for the container to start, not for PostgreSQL to accept connections. |
| 12 | 2026-10-26 | The first version is deployed after step 5, with login and items with photos, and the remaining features are added afterwards. | Deployment problems are found while the application is still small, three real users can start using it and give feedback, and a live version exists sooner. |
| 13 | 2026-10-26 | Public registration can be turned off with a setting (FR-28), and it is off for now. | The application is used privately by three people, so strangers should not be able to create accounts or use storage. It can be turned on again later. |
| 14 | 2026-10-26 | Photos are resized to fit within 1000 × 1500 pixels and converted to WebP when uploaded, and the original is not kept (FR-29, NFR-22). | Stored photos become about 100–300 KB, which makes pages faster and storage small, and hidden information such as GPS location is removed. Originals cannot be processed again later. |
| 15 | 2026-10-26 | The upload limit stays at 10 MB instead of the suggested 5 MB. | Photos from modern phones are often 3–8 MB, and the server shrinks every photo anyway, so a larger upload limit does not increase storage. |
| 16 | 2026-10-26 | The item limit is raised from 100 to 1,000 and stays a setting (NFR-11). | With resized photos and private use, storage stays small. The limit still protects the server if registration is turned on again. |
| 17 | 2026-10-26 | One photo per item in the first version; up to three photos are planned for phase 2. | Several photos would need a separate table and a more complex upload form, which would delay the first version. |