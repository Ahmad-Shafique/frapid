# core.apps table



| # | Column Name | Nullable | Data Type | Max Length | Description |
| --- | --- | --- | --- | --- | --- |
| 1 | app_name | [ ] | character varying | 100 |  |
| 2 | name | [x] | character varying | 100 |  |
| 3 | version_number | [x] | character varying | 100 |  |
| 4 | publisher | [x] | character varying | 100 |  |
| 5 | published_on | [x] | date | 0 |  |
| 6 | icon | [x] | character varying | 100 |  |
| 7 | landing_url | [x] | text | 0 |  |



**Foreign Keys**

| # | Column Name | Key Name | References |
| --- | --- | --- | --- |



**Indices**

| Index Name | Owner | Access Method | Definition | Description |
| --- | --- | --- | --- | --- |
| apps_pkey | frapid_db_user | btree | app_name |  |
| apps_app_name_uix | frapid_db_user | btree | upper(app_name::text) |  |



**Check Constraints**

| Constraint Name | Description |
| --- | --- |



**Default Values**

| # | Column Name | Default |
| --- | --- | --- |


**Triggers**

| Trigger Name | Targets | On Event | Timing | Condition | Order | Orientation | Description |
| --- | --- | --- | --- | --- | --- | --- | --- |


### Related Contents
* [Schema List](../../schemas.md)
* [Table List](../../tables.md)
* [Sequence List](../../sequences.md)
* [Table of Contents](../../README.md)
