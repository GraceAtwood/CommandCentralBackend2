{
  "TypeFullName": "CommandCentral.Authorization.PermissionGroup",
  "Definitions": [
    {
      "Name": "Developers",
      "Description": "Test desciption",
      "IsMemberOfChainOfCommand": false,
      "AccessLevels": {
        "Main": "Command",
        "Muster": "Command",
        "QuarterdeckWatchbill": "Command",
        "CommandFinancialSpecialist" : "Command"
      },
      "AccessibleSubmodules": [ "CreatePerson", "AdminTools", "EditNews", "EditFAQ" ],
      "EditablePermissionGroups": [ "Developers", "Admin", "Command Leadership", "Department Leadership", "Division Leadership" ]
    },
    {
      "Name": "Admin",
      "Description": "Test desciption",
      "IsMemberOfChainOfCommand": false,
      "AccessLevels": {
        "Main": "Command",
        "Muster": "Command",
        "QuarterdeckWatchbill": "None"
      },
      "AccessibleSubmodules": [ "CreatePerson", "AdminTools" ],
      "EditablePermissionGroups": [ "Admin" ]
    },
    {
      "Name": "Command Leadership",
      "Description": "Test desciption",
      "IsMemberOfChainOfCommand": true,
      "AccessLevels": {
        "Main": "Command",
        "Muster": "Command",
        "QuarterdeckWatchbill": "Command"
      },
      "AccessibleSubmodules": [ "CreatePerson", "AdminTools" ],
      "EditablePermissionGroups": [ "Command Leadership", "Department Leadership", "Division Leadership" ]
    },
    {
      "Name": "Department Leadership",
      "Description": "Test desciption",
      "IsMemberOfChainOfCommand": true,
      "AccessLevels": {
        "Main": "Department",
        "Muster": "Department",
        "QuarterdeckWatchbill": "Department"
      },
      "AccessibleSubmodules": [],
      "EditablePermissionGroups": [ "Department Leadership", "Division Leadership" ]
    },
    {
      "Name": "Division Leadership",
      "Description": "Test desciption",
      "IsMemberOfChainOfCommand": true,
      "AccessLevels": {
        "Main": "Division",
        "Muster": "Division",
        "QuarterdeckWatchbill": "Division"
      },
      "AccessibleSubmodules": [],
      "EditablePermissionGroups": [ "Division Leadership" ]
    }
  ]
}