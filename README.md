# Popup Dictionary

### :heavy_check_mark: Working
### :warning: Historical Project
This app was developed as an undergraduate mini-project. Its source-code does not adhere to standard conventions/practices and needs refactoring.

## Functionality
- This productivity enhancement tools was developed to help users quickly lookup meanings, pronunciation of words while they are reading, without changing the context. 
- The app works irrespective of specific application the user is currently using.
- User has options to configure shortcut keys to invoke the lookup functionality. Also, user could view his/her search history.

## Implementation 
- The underlying implementation relies on .NET clipboard-API, keyboad and mouse hooks. It listens to the key actions to invoke its operations.
- It uses a SQLite database instance to store dictionary-data and user's search history.
- The tool is configured to use an Online Api to provide pronunciation feature. (:warning: Currently broken.)

## Dependencies
- Works only on Windows.
- Building of source relies on having dictionary-database file in the build directory. This has not been automated to use Resources instead.
