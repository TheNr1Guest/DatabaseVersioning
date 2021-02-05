# Database Versioning Tool

A simple tool which allows for versioning of the database schema.

## Folder structure

The scripts path should point to a folder containing folder names which will be used as names for the databases. In each folder is a version folder. And within each version folder are scripts belonging to that version:

```
DatabaseName
|
|-- 1.0.0
|   |
|   |-- 1.sql
|   |-- 2.sql
|-- 2.0.0
    |
    |-- 1.sql
```      

## Supported providers

- MySQL

## Example

`PeopleWhoCanCode.DatabaseVersioning.Client.exe -c "Server=localhost;Uid=databaseversioner;Pwd=;" -s "C:\Git\DatabaseVersioning\Test" -p MySql`