# Mock Prototype Data

This prototype uses the 23 entities documented in `DATABASE_DICTIONARY.md`. It does not add a separate user, permission, cash-advance, report, or liquidation table.

## Seed volumes

| Entity | Rows |
|---|---:|
| TRAVELLER | 28 |
| DEPARTMENT | 10 |
| TRIP | 40 |
| TRIP_EXPENSE | 100 |
| APPROVAL | 36 |
| TRIP_PROCESS | 520 |
| TRIP_ACTION | 137 |
| AUDIT_LOG | 60 |

The seed also includes 105 lookup rows, 14 regions, 14 destinations, 74 flights, 127 arrangements, 45 comments, 22 attachments, 13 workflow steps, active and inactive reference values, and records with optional fields populated and omitted.

## Default administrator

| Field | Value |
|---|---|
| Full name | Marq Paul Gonzales |
| Employee number | `01023712` |
| Email | `marqpaulgonzales22@gmail.com` |
| Mobile | `09165087795` |
| Nickname | Marq |
| Department | IT Department |
| Role | Administrator |
| Mock password | `123` |
| Account status | Active |

The password is intentionally insecure mock data for local prototype testing. The seed stores it in `TRAVELLER.PIN_HASH` as `INSECURE_MOCK_PLAINTEXT:123` only because the requested temporary login must compare the entered value directly. This deliberately does **not** satisfy the production security rule described for `PIN_HASH`; it must never be reused in production or treated as a real password implementation.

## Additional test users

All active seeded users use the same local-only mock password `123`.

| Employee number | User | Primary test role | Useful scenario |
|---|---|---|---|
| `01024001` | Liza Mercado | Employee / Requester | Own applications and requester permissions |
| `01024002` | Noel Bautista | Immediate Manager / Department Approver | Recommending approval |
| `01024004` | Victor Ong | Travel Administrator (GA) | Arrangements and process-wide travel support |
| `01024005` | Rina Velasco | Human Resources Reviewer | HR declaration and approval |
| `01024006` | Felix Navarro | Finance Reviewer | Accounting review, release, and liquidation |
| `01024007` | Isabel Cruz | Vice President Approver | Executive approval level |
| `01024008` | Dominic Reyes | CEO Approver | Final approval level |
| `01024009` | Tina Yu | Business Trip Report Reviewer | Report review |
| `01024010` | Dr. Paolo Mercado | Clinic Reviewer | Medical-supply arrangement |
| `01024011` | Maya Lim | Read-only User | No owned trips and no assigned approvals |
| `01024027` | Trent Co | Inactive employee | Login rejection and inactive-account handling |

## Test records

- `BTS-2026-0001` / `TRIP_ID 9001`: completed record with itinerary, flights, arrangements, multiple expenses, comments, attachment, workflow, approval, status, and audit history.
- `BTS-2026-0024` / `TRIP_ID 9024`: pending record with an active process assigned to Marq for administrator approval testing.
- `APP-2026-0040` / `TRIP_ID 9040`: Marq-owned unsubmitted draft with no expense or comment rows, suitable for edit, delete, submit, and empty-state testing.
- Maya Lim has no owned trips or assigned workflow records, providing a user-level empty state.
- Cancelled and rejected requests include realistic reasons and ordered status transitions.
- Completed trips have expenses at the `LIQUIDATED` stage; ongoing trips use `RELEASED`; upcoming trips use `REVIEWED`; pending requests mix `DECLARED` and `REVIEWED`.

## Persistence and reset

`MockDatabaseService` merges the six JSON seed shards and saves the complete snapshot to:

```text
localStorage["bts.prototype.mockDatabase.v1"]
```

Initialization only writes the seed when that key is absent, invalid, or fails integrity validation, so normal page refreshes do not overwrite changes.

The current signed-in user is stored separately with only UI-required identity and role information:

```text
localStorage["bts.prototype.currentUser.v1"]
```

`AuthSessionService.ResetMockDatabaseAsync()` restores the original seed and signs Marq back in as the default administrator.

## Authorization model

The documented schema contains `ROLE`, `TRAVELLER_ROLE`, and role-to-module mapping through `ROLE.MODULE_ID`; it does not contain a separate permission table. Prototype permission checks therefore derive menu and action capabilities from active role assignments rather than inventing a permission entity.
