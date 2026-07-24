# Database Dictionary

**System:** TPC Business Travel System  
**Report:** Database Structure  
**Source worksheet:** Database (Oracle Database Tables)  
**Classification:** L2 - Internal Use Only  
**Generated:** July 24, 2026

## 1. Executive Summary

The database design supports the complete business travel lifecycle: system configuration, reference data, organizational structure, users and roles, trip applications, destinations and flights, travel arrangements, expenses, attachments, workflow processing, approvals, user actions, and audit history.

The worksheet defines 23 Oracle database tables. The design uses numeric surrogate primary keys for most business entities, lookup-driven status and type values, foreign keys for referential integrity, timestamped records, soft-delete or deactivation policies where history must be retained, and append-only logs for approval and audit evidence.

Major design areas: configuration and reference data; organization, identity, access, and sessions; trip application and travel details; expenses, comments, and attachments; workflow and approvals; operational and audit history.

## 2. Database Platform and Common Conventions

- **Platform:** Oracle Database
- **Primary key pattern:** NUMBER(12) surrogate identifiers, except SESSION_ID which uses VARCHAR2(64)
- **Date/time pattern:** TIMESTAMP WITH TIME ZONE; workflow effective dates use DATE
- **Creation timestamps:** Commonly default to SYSTIMESTAMP
- **Active-state pattern:** IS_ACTIVE CHAR(1), checked to Y or N
- **Soft-delete pattern:** DELETED_AT and, where applicable, DELETED_BY fields
- **Reference-data pattern:** LOOKUP table values linked through foreign keys
- **Currency pattern:** CHAR(3) currency codes and NUMBER amounts/rates
- **Audit pattern:** Append-only action and change-history records
- **File-storage pattern:** Attachment metadata plus BLOB content and SHA-256 hash

## 3. Table Inventory

### A. CONFIGURATION AND REFERENCE DATA

#### 1. APP_SETTING
**Purpose:** Stores configurable application settings.

**Columns**
- `SETTING_ID` — `NUMBER(12)`; primary key, not null
- `SETTING_KEY` — `VARCHAR2(100)`; unique, not null
- `SETTING_VALUE` — `VARCHAR2(2000)`; not null
- `DESCRIPTION` — `VARCHAR2(500)`; nullable
- `UPDATED_BY_TRAVELLER_ID` — `NUMBER(12)`; foreign key, not null
- `UPDATED_AT` — `TIMESTAMP WITH TIME ZONE`; default SYSTIMESTAMP, not null
- `CREATED_AT` — `TIMESTAMP WITH TIME ZONE`; default SYSTIMESTAMP, not null

**Relationships**
- UPDATED_BY_TRAVELLER_ID → TRAVELLER.TRAVELLER_ID

**Lifecycle**
- Create/update only; record changes in AUDIT_LOG.
- Hard-delete only when the setting is unused.

#### 2. LOOKUP
**Purpose:** Central reference table for reusable codes, names, types, and statuses.

**Columns**
- `LOOKUP_ID` — `NUMBER(12)`; primary key, not null
- `LOOKUP_TYPE` — `VARCHAR2(50)`; not null
- `LOOKUP_CODE` — `VARCHAR2(50)`; not null
- `LOOKUP_NAME` — `VARCHAR2(100)`; not null
- `DESCRIPTION` — `VARCHAR2(500)`; nullable
- `SORT_ORDER` — `NUMBER(4)`; default 0, checked >= 0, not null
- `IS_ACTIVE` — `CHAR(1)`; default Y, checked Y/N, not null
- `CREATED_AT` — `TIMESTAMP WITH TIME ZONE`; default SYSTIMESTAMP, not null
- `UPDATED_AT` — `TIMESTAMP WITH TIME ZONE`; nullable

**Constraints**
- Unique combination: LOOKUP_TYPE + LOOKUP_CODE

**Lifecycle**
- Deactivate referenced values instead of deleting them.
- Audit create, update, activation, and deactivation events.

#### 3. DEPARTMENT
**Purpose:** Maintains the organizational department master list.

**Columns**
- `DEPARTMENT_ID` — `NUMBER(12)`; primary key, not null
- `DEPARTMENT_CODE` — `VARCHAR2(30)`; unique, not null
- `DEPARTMENT_NAME` — `VARCHAR2(100)`; not null
- `IS_ACTIVE` — `CHAR(1)`; default Y, checked Y/N, not null
- `CREATED_AT` — `TIMESTAMP WITH TIME ZONE`; default SYSTIMESTAMP, not null
- `UPDATED_AT` — `TIMESTAMP WITH TIME ZONE`; nullable

**Lifecycle**
- Deactivate rather than delete when referenced.

#### 4. REGION
**Purpose:** Represents a city/province/country combination used by destinations.

**Columns**
- `REGION_ID` — `NUMBER(12)`; primary key, not null
- `CITY_ID` — `NUMBER(12)`; foreign key, not null
- `PROVINCE_ID` — `NUMBER(12)`; foreign key, not null
- `COUNTRY_ID` — `NUMBER(12)`; foreign key, not null
- `IS_ACTIVE` — `CHAR(1)`; default Y, checked Y/N, not null
- `CREATED_AT` — `TIMESTAMP WITH TIME ZONE`; default SYSTIMESTAMP, not null
- `UPDATED_AT` — `TIMESTAMP WITH TIME ZONE`; nullable

**Relationships**
- CITY_ID → LOOKUP.LOOKUP_ID [CITY]
- PROVINCE_ID → LOOKUP.LOOKUP_ID [PROVINCE]
- COUNTRY_ID → LOOKUP.LOOKUP_ID [COUNTRY]

**Constraints**
- Unique combination: CITY_ID + PROVINCE_ID + COUNTRY_ID

**Lifecycle**
- Deactivate instead of deleting when referenced.

### B. IDENTITY, ACCESS, AND SESSION MANAGEMENT

#### 5. ROLE
**Purpose:** Defines application roles by module.

**Columns**
- `ROLE_ID` — `NUMBER(12)`; primary key, not null
- `ROLE_CODE` — `VARCHAR2(50)`; unique, not null
- `ROLE_NAME` — `VARCHAR2(80)`; not null
- `MODULE_ID` — `NUMBER(12)`; foreign key, not null
- `DESCRIPTION` — `VARCHAR2(500)`; nullable
- `IS_ACTIVE` — `CHAR(1)`; default Y, checked Y/N, not null
- `CREATED_AT` — `TIMESTAMP WITH TIME ZONE`; default SYSTIMESTAMP, not null
- `UPDATED_AT` — `TIMESTAMP WITH TIME ZONE`; nullable

**Relationships**
- MODULE_ID → LOOKUP.LOOKUP_ID [MODULE]

**Lifecycle**
- Deactivate instead of deleting when referenced.

#### 6. TRAVELLER
**Purpose:** Stores user/traveller identity, contact, department, and login information.

**Security note:** PIN_HASH stores only a salted adaptive hash, never the plaintext PIN.

**Columns**
- `TRAVELLER_ID` — `NUMBER(12)`; primary key, not null
- `EMPLOYEE_NO` — `VARCHAR2(30)`; unique, not null
- `PIN_HASH` — `VARCHAR2(255)`; not null
- `EMAIL` — `VARCHAR2(254)`; unique, not null
- `FULL_NAME` — `VARCHAR2(150)`; not null
- `NICKNAME` — `VARCHAR2(80)`; nullable
- `DEPARTMENT_ID` — `NUMBER(12)`; foreign key, not null
- `LOCAL_NO` — `VARCHAR2(30)`; nullable
- `MOBILE_NO` — `VARCHAR2(30)`; nullable
- `IS_ACTIVE` — `CHAR(1)`; default Y, checked Y/N, not null
- `LAST_LOGIN_AT` — `TIMESTAMP WITH TIME ZONE`; nullable
- `CREATED_AT` — `TIMESTAMP WITH TIME ZONE`; default SYSTIMESTAMP, not null
- `UPDATED_AT` — `TIMESTAMP WITH TIME ZONE`; nullable

**Relationships**
- DEPARTMENT_ID → DEPARTMENT.DEPARTMENT_ID

**Lifecycle**
- Deactivate to block login while retaining historical references.

#### 7. TRAVELLER_ROLE
**Purpose:** Assigns roles to travellers and records assignment/revocation history.

**Columns**
- `TRAVELLER_ROLE_ID` — `NUMBER(12)`; primary key, not null
- `TRAVELLER_ID` — `NUMBER(12)`; foreign key, not null
- `ROLE_ID` — `NUMBER(12)`; foreign key, not null
- `ASSIGNED_AT` — `TIMESTAMP WITH TIME ZONE`; default SYSTIMESTAMP, not null
- `ASSIGNED_BY_TRAVELLER_ID` — `NUMBER(12)`; foreign key, not null
- `REVOKED_AT` — `TIMESTAMP WITH TIME ZONE`; nullable
- `REVOKED_BY_TRAVELLER_ID` — `NUMBER(12)`; foreign key, nullable

**Relationships**
- Traveller, role, assigning traveller, and revoking traveller references

**Constraints**
- Unique combination: TRAVELLER_ID + ROLE_ID

**Lifecycle**
- Revoke through the revocation fields.
- Hard-delete only an unused setup mistake.

#### 8. TRAVELLER_SESSION
**Purpose:** Tracks authenticated user sessions and trusted-device state.

**Columns**
- `SESSION_ID` — `VARCHAR2(64)`; primary key, not null
- `TRAVELLER_ID` — `NUMBER(12)`; foreign key, not null
- `TOKEN_HASH` — `VARCHAR2(128)`; not null
- `TRUSTED_DEVICE` — `CHAR(1)`; default N, checked Y/N, not null
- `EXPIRES_AT` — `TIMESTAMP WITH TIME ZONE`; not null
- `REVOKED_AT` — `TIMESTAMP WITH TIME ZONE`; nullable
- `CREATED_AT` — `TIMESTAMP WITH TIME ZONE`; default SYSTIMESTAMP, not null

**Relationships**
- TRAVELLER_ID → TRAVELLER.TRAVELLER_ID

**Lifecycle**
- Use expiration and revocation; purge later according to retention policy.

### C. TRIP APPLICATION AND TRAVEL DETAILS

#### 9. TRIP
**Purpose:** Core travel application record for an applicant and traveller.

**Columns**
- `TRIP_ID` — `NUMBER(12)`; primary key, not null
- `APPLICATION_NO` — `VARCHAR2(30)`; not null
- `TRIP_NO` — `VARCHAR2(30)`; unique, nullable until submission
- `APPLICANT_ID` — `NUMBER(12)`; foreign key, not null
- `TRAVELLER_ID` — `NUMBER(12)`; foreign key, not null
- `IS_PRIMARY` — `CHAR(1)`; default N, checked Y/N, not null
- `TITLE` — `VARCHAR2(200)`; not null
- `PURPOSE` — `VARCHAR2(1000)`; not null
- `TRIP_CATEGORY_ID` — `NUMBER(12)`; foreign key, not null
- `BUDGET_JUSTIFICATION` — `VARCHAR2(1000)`; nullable
- `COST_CENTER_ID` — `NUMBER(12)`; foreign key, not null
- `STATUS_ID` — `NUMBER(12)`; foreign key, not null
- `SUBMITTED_AT` — `TIMESTAMP WITH TIME ZONE`; nullable
- `CREATED_AT` — `TIMESTAMP WITH TIME ZONE`; default SYSTIMESTAMP, not null
- `UPDATED_AT` — `TIMESTAMP WITH TIME ZONE`; nullable

**Relationships**
- APPLICANT_ID and TRAVELLER_ID → TRAVELLER
- TRIP_CATEGORY_ID → LOOKUP [TRIP_CATEGORY]
- COST_CENTER_ID → LOOKUP [COST_CENTER]
- STATUS_ID → LOOKUP [TRIP_STATUS]

**Constraints**
- Unique TRIP_NO
- Unique APPLICATION_NO + TRAVELLER_ID
- One primary traveller per application through a function-based unique index

**Lifecycle**
- Manage lifecycle through STATUS_ID.
- Hard-delete only an unsubmitted abandoned draft.

#### 10. DESTINATION
**Purpose:** Master list of selectable travel destinations.

**Columns**
- `DESTINATION_ID` — `NUMBER(12)`; primary key, not null
- `DESTINATION_NAME` — `VARCHAR2(150)`; not null
- `REGION_ID` — `NUMBER(12)`; foreign key, not null
- `TIMEZONE_NAME` — `VARCHAR2(64)`; not null
- `LATITUDE` — `NUMBER(9,6)`; checked from -90 to 90, not null
- `LONGITUDE` — `NUMBER(9,6)`; checked from -180 to 180, not null
- `IS_ACTIVE` — `CHAR(1)`; default Y, checked Y/N, not null
- `CREATED_AT` — `TIMESTAMP WITH TIME ZONE`; default SYSTIMESTAMP, not null
- `UPDATED_AT` — `TIMESTAMP WITH TIME ZONE`; nullable

**Relationships**
- REGION_ID → REGION.REGION_ID

**Constraints**
- Unique combination: DESTINATION_NAME + REGION_ID

**Lifecycle**
- Deactivate to prevent new selection while retaining historical trips.

#### 11. TRIP_DESTINATION
**Purpose:** Links a trip to one or more destinations and planned travel dates.

**Columns**
- `TRIP_DESTINATION_ID` — `NUMBER(12)`; primary key, not null
- `TRIP_ID` — `NUMBER(12)`; foreign key, not null
- `DESTINATION_ID` — `NUMBER(12)`; foreign key, not null
- `ARRIVAL_AT` — `TIMESTAMP WITH TIME ZONE`; not null
- `DEPARTURE_AT` — `TIMESTAMP WITH TIME ZONE`; not null
- `REMARKS` — `VARCHAR2(1000)`; nullable
- `CREATED_AT` — `TIMESTAMP WITH TIME ZONE`; default SYSTIMESTAMP, not null
- `UPDATED_AT` — `TIMESTAMP WITH TIME ZONE`; nullable

**Relationships**
- TRIP_ID → TRIP.TRIP_ID
- DESTINATION_ID → DESTINATION.DESTINATION_ID

**Lifecycle**
- Editable/hard-deletable while the trip is a draft.
- Controlled updates with AUDIT_LOG after submission.

#### 12. FLIGHT
**Purpose:** Stores flight itinerary and booking details for a trip.

**Columns**
- `FLIGHT_ID` — `NUMBER(12)`; primary key, not null
- `TRIP_ID` — `NUMBER(12)`; foreign key, not null
- `CARRIER_CODE` — `VARCHAR2(10)`; not null
- `FLIGHT_NO` — `VARCHAR2(30)`; not null
- `DEPARTURE_DESTINATION_ID` — `NUMBER(12)`; foreign key, not null
- `ARRIVAL_DESTINATION_ID` — `NUMBER(12)`; foreign key, not null
- `DEPARTURE_AT` — `TIMESTAMP WITH TIME ZONE`; not null
- `ARRIVAL_AT` — `TIMESTAMP WITH TIME ZONE`; not null
- `FLIGHT_TYPE_ID` — `NUMBER(12)`; foreign key, not null
- `BOOKING_REFERENCE` — `VARCHAR2(30)`; nullable
- `REMARKS` — `VARCHAR2(1000)`; nullable
- `CREATED_AT` — `TIMESTAMP WITH TIME ZONE`; default SYSTIMESTAMP, not null
- `UPDATED_AT` — `TIMESTAMP WITH TIME ZONE`; nullable

**Relationships**
- TRIP_ID → TRIP
- Departure and arrival destination IDs → DESTINATION
- FLIGHT_TYPE_ID → LOOKUP [FLIGHT_TYPE]

**Constraints**
- ARRIVAL_AT must be later than DEPARTURE_AT.

**Lifecycle**
- Editable/hard-deletable while the trip is a draft.
- Controlled updates with AUDIT_LOG after submission.

#### 13. ARRANGEMENT
**Purpose:** Tracks hotel, shuttle, host, and other trip arrangements.

**Columns**
- `ARRANGEMENT_ID` — `NUMBER(12)`; primary key, not null
- `TRIP_ID` — `NUMBER(12)`; foreign key, not null
- `DESTINATION_ID` — `NUMBER(12)`; foreign key, not null
- `ARRANGEMENT_TYPE_ID` — `NUMBER(12)`; foreign key, not null
- `ARRANGEMENT_STATUS_ID` — `NUMBER(12)`; foreign key, not null
- `TITLE` — `VARCHAR2(150)`; not null
- `PROVIDER_NAME` — `VARCHAR2(150)`; nullable
- `DESCRIPTION` — `VARCHAR2(1000)`; nullable
- `CONTACT_NAME` — `VARCHAR2(150)`; nullable
- `CONTACT_NO` — `VARCHAR2(30)`; nullable
- `MOBILE_NO` — `VARCHAR2(30)`; nullable
- `EMAIL` — `VARCHAR2(254)`; nullable
- `CREATED_AT` — `TIMESTAMP WITH TIME ZONE`; default SYSTIMESTAMP, not null
- `UPDATED_AT` — `TIMESTAMP WITH TIME ZONE`; nullable

**Relationships**
- TRIP_ID → TRIP; DESTINATION_ID → DESTINATION
- Type and status IDs → LOOKUP

**Lifecycle**
- Manage through ARRANGEMENT_STATUS_ID.
- Do not delete a genuine cancelled arrangement.

### D. EXPENSES, COMMENTS, AND FILES

#### 14. TRIP_EXPENSE
**Purpose:** Records trip expenses, original currency, exchange rate, and base amount.

**Columns**
- `TRIP_EXPENSE_ID` — `NUMBER(12)`; primary key, not null
- `TRIP_ID` — `NUMBER(12)`; foreign key, not null
- `EXPENSE_TYPE_ID` — `NUMBER(12)`; foreign key, not null
- `DESCRIPTION` — `VARCHAR2(500)`; nullable
- `ORIGINAL_AMOUNT` — `NUMBER(18,2)`; checked >= 0, not null
- `ORIGINAL_CURRENCY` — `CHAR(3)`; not null
- `EXCHANGE_RATE` — `NUMBER(18,8)`; checked > 0, not null
- `BASE_AMOUNT` — `NUMBER(18,2)`; checked >= 0, not null
- `BASE_CURRENCY` — `CHAR(3)`; not null
- `EXPENSE_STAGE_ID` — `NUMBER(12)`; foreign key, not null
- `CREATED_BY_TRAVELLER_ID` — `NUMBER(12)`; foreign key, not null
- `CREATED_AT` — `TIMESTAMP WITH TIME ZONE`; default SYSTIMESTAMP, not null
- `UPDATED_AT` — `TIMESTAMP WITH TIME ZONE`; nullable

**Lifecycle**
- Manage through EXPENSE_STAGE_ID.
- Hard-delete only draft mistakes; retain submitted accounting history.

#### 15. TRIP_COMMENT
**Purpose:** Stores traveller comments associated with a trip.

**Columns**
- `COMMENT_ID` — `NUMBER(12)`; primary key, not null
- `TRIP_ID` — `NUMBER(12)`; foreign key, not null
- `COMMENT_TEXT` — `VARCHAR2(2000)`; not null
- `CREATED_BY_TRAVELLER_ID` — `NUMBER(12)`; foreign key, not null
- `CREATED_AT` — `TIMESTAMP WITH TIME ZONE`; default SYSTIMESTAMP, not null
- `UPDATED_AT` — `TIMESTAMP WITH TIME ZONE`; nullable
- `DELETED_AT` — `TIMESTAMP WITH TIME ZONE`; nullable

**Lifecycle**
- Soft-delete using DELETED_AT.

#### 16. ATTACHMENT
**Purpose:** Stores files for a trip, traveller, expense, or comment.

**Columns**
- `ATTACHMENT_ID` — `NUMBER(12)`; primary key, not null
- `TRIP_ID` — `NUMBER(12)`; nullable foreign key
- `TRAVELLER_ID` — `NUMBER(12)`; nullable foreign key
- `TRIP_EXPENSE_ID` — `NUMBER(12)`; nullable foreign key
- `COMMENT_ID` — `NUMBER(12)`; nullable foreign key
- `FILE_NAME` — `VARCHAR2(255)`; not null
- `MIME_TYPE` — `VARCHAR2(100)`; not null
- `FILE_SIZE_BYTES` — `NUMBER(18)`; checked >= 0, not null
- `ATTACHMENT` — `BLOB`; not null
- `SHA256_CHECKSUM` — `CHAR(64)`; not null
- `UPLOADED_BY_TRAVELLER_ID` — `NUMBER(12)`; foreign key, not null
- `UPLOADED_AT` — `TIMESTAMP WITH TIME ZONE`; default SYSTIMESTAMP, not null
- `DELETED_AT` — `TIMESTAMP WITH TIME ZONE`; nullable
- `DELETED_BY_TRAVELLER_ID` — `NUMBER(12)`; foreign key, nullable

**Constraints**
- Exactly one parent among TRIP_ID, TRAVELLER_ID, TRIP_EXPENSE_ID, and COMMENT_ID must be populated.

**Lifecycle**
- Soft-delete attachments.
- Purge BLOB data only through an approved retention process.

### E. WORKFLOW AND APPROVALS

#### 17. WORKFLOW
**Purpose:** Defines versioned workflows, optionally by trip category.

**Columns**
- `WORKFLOW_ID` — `NUMBER(12)`; primary key, not null
- `WORKFLOW_CODE` — `VARCHAR2(50)`; not null
- `VERSION_NO` — `NUMBER(4)`; checked > 0, not null
- `WORKFLOW_NAME` — `VARCHAR2(150)`; not null
- `TRIP_CATEGORY_ID` — `NUMBER(12)`; nullable foreign key
- `EFFECTIVE_FROM` — `DATE`; not null
- `EFFECTIVE_TO` — `DATE`; nullable
- `IS_ACTIVE` — `CHAR(1)`; default Y, checked Y/N, not null
- `CREATED_AT` — `TIMESTAMP WITH TIME ZONE`; default SYSTIMESTAMP, not null
- `UPDATED_AT` — `TIMESTAMP WITH TIME ZONE`; nullable

**Constraints**
- Unique WORKFLOW_CODE + VERSION_NO
- EFFECTIVE_TO must be null or on/after EFFECTIVE_FROM

**Lifecycle**
- Versioned and immutable after first use; deactivate superseded versions.

#### 18. WORKFLOW_ACTION
**Purpose:** Master list of actions such as approve, reject, hold, reverse, or cancel.

**Columns**
- `ACTION_ID` — `NUMBER(12)`; primary key, not null
- `ACTION_CODE` — `VARCHAR2(30)`; unique, not null
- `ACTION_NAME` — `VARCHAR2(100)`; not null
- `DESCRIPTION` — `VARCHAR2(500)`; nullable
- `IS_ACTIVE` — `CHAR(1)`; default Y, checked Y/N, not null
- `CREATED_AT` — `TIMESTAMP WITH TIME ZONE`; default SYSTIMESTAMP, not null
- `UPDATED_AT` — `TIMESTAMP WITH TIME ZONE`; nullable

**Lifecycle**
- Deactivate to prevent new use while retaining history.

#### 19. WORKFLOW_STEP
**Purpose:** Defines the ordered approval steps within a workflow version.

**Columns**
- `WORKFLOW_STEP_ID` — `NUMBER(12)`; primary key, not null
- `WORKFLOW_ID` — `NUMBER(12)`; foreign key, not null
- `SEQUENCE_NO` — `NUMBER(4)`; checked > 0, not null
- `STEP_CODE` — `VARCHAR2(50)`; not null
- `STEP_NAME` — `VARCHAR2(150)`; not null
- `APPROVER_ROLE_ID` — `NUMBER(12)`; foreign key, not null
- `REQUIRED_ACTION_ID` — `NUMBER(12)`; foreign key, not null
- `DESCRIPTION` — `VARCHAR2(500)`; nullable
- `CREATED_AT` — `TIMESTAMP WITH TIME ZONE`; default SYSTIMESTAMP, not null

**Constraints**
- Unique sequence per workflow
- Unique step code per workflow

**Lifecycle**
- Immutable after publication; create a new workflow version for changes.

#### 20. TRIP_PROCESS
**Purpose:** Runtime instance of a workflow step for a specific trip.

**Columns**
- `TRIP_PROCESS_ID` — `NUMBER(12)`; primary key, not null
- `TRIP_ID` — `NUMBER(12)`; foreign key, not null
- `WORKFLOW_STEP_ID` — `NUMBER(12)`; foreign key, not null
- `SEQUENCE_NO_SNAPSHOT` — `NUMBER(4)`; not null
- `STEP_STATUS_ID` — `NUMBER(12)`; foreign key, not null
- `ASSIGNED_TO_TRAVELLER_ID` — `NUMBER(12)`; foreign key, nullable
- `STARTED_AT` — `TIMESTAMP WITH TIME ZONE`; nullable
- `COMPLETED_AT` — `TIMESTAMP WITH TIME ZONE`; nullable

**Relationships**
- STEP_STATUS_ID → LOOKUP [PROCESS_STATUS]

**Constraints**
- COMPLETED_AT must be null or not earlier than STARTED_AT.

**Lifecycle**
- Update during processing; preserve as history after completion.

#### 21. APPROVAL
**Purpose:** Records an approver's decision for a trip process step.

**Columns**
- `APPROVAL_ID` — `NUMBER(12)`; primary key, not null
- `TRIP_PROCESS_ID` — `NUMBER(12)`; foreign key, not null
- `APPROVER_TRAVELLER_ID` — `NUMBER(12)`; foreign key, not null
- `ACTION_ID` — `NUMBER(12)`; foreign key, not null
- `REMARKS` — `VARCHAR2(1000)`; nullable
- `DECIDED_AT` — `TIMESTAMP WITH TIME ZONE`; not null

**Constraints**
- Unique approver per trip process step.

**Lifecycle**
- Append-only historical decision; do not update or delete.

### F. OPERATIONAL AND AUDIT HISTORY

#### 22. TRIP_ACTION
**Purpose:** Records every trip action and resulting status transition.

**Columns**
- `TRIP_ACTION_ID` — `NUMBER(12)`; primary key, not null
- `TRIP_ID` — `NUMBER(12)`; foreign key, not null
- `ACTION_ID` — `NUMBER(12)`; foreign key, not null
- `ACTION_BY_TRAVELLER_ID` — `NUMBER(12)`; foreign key, not null
- `ACTION_AT` — `TIMESTAMP WITH TIME ZONE`; default SYSTIMESTAMP, not null
- `REMARKS` — `VARCHAR2(1000)`; nullable
- `FROM_STATUS_ID` — `NUMBER(12)`; foreign key, not null
- `TO_STATUS_ID` — `NUMBER(12)`; foreign key, not null

**Lifecycle**
- Append-only trip/status history; do not update or delete.

#### 23. AUDIT_LOG
**Purpose:** Captures generic entity changes with before-and-after JSON values.

**Columns**
- `AUDIT_LOG_ID` — `NUMBER(12)`; primary key, not null
- `ENTITY_TYPE` — `VARCHAR2(50)`; not null
- `ENTITY_ID` — `NUMBER(12)`; not null
- `ACTION_CODE` — `VARCHAR2(30)`; not null
- `OLD_VALUES` — `CLOB`; nullable
- `NEW_VALUES` — `CLOB`; nullable
- `PERFORMED_BY_TRAVELLER_ID` — `NUMBER(12)`; foreign key, not null
- `PERFORMED_AT` — `TIMESTAMP WITH TIME ZONE`; default SYSTIMESTAMP, not null
- `CLIENT_IP` — `VARCHAR2(45)`; nullable

**Constraints**
- OLD_VALUES must be null or valid JSON.
- NEW_VALUES must be null or valid JSON.

**Lifecycle**
- Append-only change history; the application must not update or delete it.

## 4. High-Level Relationship Model

```text
DEPARTMENT -> TRAVELLER -> TRAVELLER_ROLE -> ROLE
                         -> TRAVELLER_SESSION
                         -> TRIP (as applicant and/or traveller)

LOOKUP provides types and statuses for regions, roles/modules, trips, cost
centers, destinations, flights, arrangements, expenses, processes, and actions.

TRIP -> TRIP_DESTINATION -> DESTINATION -> REGION -> LOOKUP
     -> FLIGHT -> DESTINATION
     -> ARRANGEMENT -> DESTINATION
     -> TRIP_EXPENSE
     -> TRIP_COMMENT
     -> ATTACHMENT
     -> TRIP_PROCESS -> WORKFLOW_STEP -> WORKFLOW
                     -> APPROVAL
     -> TRIP_ACTION

AUDIT_LOG records changes across business entities and identifies the traveller
who performed each change.
```

## 5. Key Data-Integrity and Security Controls

1. Unique employee numbers, emails, role codes, department codes, and setting keys prevent duplicate master data.
2. Foreign keys tie applications, travellers, destinations, costs, approvals, statuses, and workflow steps to valid parent records.
3. Y/N checks standardize active, primary, and trusted-device indicators.
4. Date checks protect workflow effective periods and process completion order.
5. Amount, rate, coordinate, sequence, and file-size checks reject invalid values.
6. One-primary-traveller logic protects multi-traveller applications.
7. Attachment ownership is exclusive to exactly one supported parent entity.
8. PIN and session tokens are stored as hashes rather than plaintext secrets.
9. SHA-256 checksums support attachment integrity verification.
10. Append-only approval, action, and audit tables preserve accountability.
11. Soft-delete/deactivation policies retain historical and accounting evidence.
12. AUDIT_LOG before-and-after values are validated as JSON when populated.

## 6. Lifecycle and Retention Summary

**Deactivate rather than delete**

- LOOKUP, DEPARTMENT, REGION, ROLE, TRAVELLER, DESTINATION, WORKFLOW_ACTION, and superseded WORKFLOW versions.

**Soft-delete**

- TRIP_COMMENT and ATTACHMENT.

**Hard-delete allowed only in limited draft/setup cases**

- Unused APP_SETTING records, unused TRAVELLER_ROLE setup mistakes, unsubmitted abandoned TRIP drafts, and draft travel/expense details.

**Immutable or append-only after use**

- Published WORKFLOW_STEP definitions, APPROVAL decisions, TRIP_ACTION, and AUDIT_LOG.

**Retention-sensitive content**

- Session records should be purged according to retention policy.
- Attachment BLOBs should be purged only under an approved process.
- Submitted accounting and trip records should be retained for history.

## 7. Design Observations

**Strengths**

- Clear separation of master, transactional, workflow, and audit data.
- Strong use of foreign keys, uniqueness rules, and check constraints.
- Versioned workflow model supports controlled future changes.
- Detailed history supports accountability and operational traceability.
- Currency, attachments, multiple travellers, and multiple destinations are represented explicitly.

**Items for implementation review**

- Confirm all lookup types and seed values before deployment.
- Confirm the exact retention periods for sessions, files, comments, trip records, and audit history.
- Confirm encryption, access control, and backup requirements for BLOB files, PIN hashes, token hashes, personal data, and financial information.
- Validate indexes for high-volume foreign keys and common search fields.
- Review the worksheet's TRIP_DESTINATION departure/arrival check wording; the intended rule should normally require departure after arrival at that destination.
- Standardize whether all UPDATED_AT columns should be populated by triggers or application logic.
- Define how lookup-category integrity is enforced so that a foreign key also uses the expected LOOKUP_TYPE.

## 8. Conclusion

The proposed Oracle schema provides a solid foundation for the TPC Business Travel System. It covers identity and access, application details, itinerary, arrangements, expenses, document storage, versioned approvals, and auditability. The lifecycle policies favor preservation of business history, which is appropriate for travel, approval, and accounting records. Before implementation, the project team should finalize lookup seed data, retention periods, security controls, indexing, and the identified date-rule clarification.

_This document is the project’s current database reference. Review every implementation change against it. When a request conflicts with or exposes a gap in the schema, align the schema before implementation._
