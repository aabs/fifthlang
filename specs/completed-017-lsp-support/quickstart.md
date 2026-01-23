# Quickstart: Fifth Language Server

## Goal
Connect an editor to the Fifth language server to receive diagnostics, hover, completion, and go-to-definition.

## Steps
1. Build the Fifth language server executable from the repository.
2. Configure your editor to start the server using stdio transport.
3. Open a `.5th` file and confirm:
   - Diagnostics appear while typing
   - Hover shows type/signature
   - Completion suggests symbols and keywords
   - Go-to-definition navigates within the workspace

## Expected Results
- Diagnostics update without saving files.
- Hover and definition requests respond quickly on typical projects.
- Completions appear in relevant contexts.
