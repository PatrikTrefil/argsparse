## Tests for Argparser API

### Used Framework

- NUnit
- If you do not have Visual Studio, tests can be run by calling `dotnet tests` in the root folder of the repository

### Tests structure

All tests are in one project, they are however devided into several folders and classes.
The structure is as follows:
- Configuration tests
    - Flag Configuration
    - Option Configuration
    - Argument Configuration
    - Whole Parser Configuration
- Parsing tests
    - Parsing of flags
    - Parsing of options
    - Parsing of arguments
    - Parsing of mixed input (flags, options and arguments together)
- Special tests (special or advanced functionality)
    - Subparsers

There are no tests for automatic help printing and formatting, because there are no examples of expected output and I did not want to add work with test updating for the API authors by defining my own help message format.


