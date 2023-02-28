
# Our custom argsparse

## What we want to support

Keep it simple, stupid! It's a school project.

- options
  - long, short
  - synonyms
    - add retroactively? Probably not
  - values
    - optional / default
    - convert to type via user-provided function
    - choose from list
  - positional arguments
    - as simple list?
    - separated
- subparsers
  - options separation? Try to find unrecognized options in parent / child parser ?
  - refer to context of parent command
    - Fluent syntax with cost of confusion with And() and Furthermore()
- Two syntaxes
  - fluent
  - create standalone builder and add it
    - reusability
  - no way to discrern syntactically one from the other without huge cost in duplication / boilerplate
    - check at runtime
      -  InvalidBuilderContextTraversalException