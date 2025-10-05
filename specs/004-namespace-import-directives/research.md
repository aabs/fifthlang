# Research on Namespace Import Directives

## Design Choices

This document outlines the design choices regarding namespace import directives in the fifthlang project.

### Explicit Enumeration of File Names

Explicit enumeration of file names is a method where each file is listed individually. This approach provides clarity and control over which files are included in the project.

### Compatibility with Globbing and MSBuild

It is important to note that explicit enumeration of file names is compatible with globbing and MSBuild project management. Both approaches can be used together effectively, allowing for a flexible and manageable project structure.

### Conclusion

In summary, utilizing both explicit enumeration and globbing offers a robust solution for managing files in the fifthlang project.