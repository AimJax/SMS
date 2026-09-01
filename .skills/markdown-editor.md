# Markdown Editor Skill

## Purpose
Enables reading, creating, editing, and managing Markdown (.md) files in the workspace.

## Triggers
- User asks to view, read, edit, create, or work with .md files
- User asks for a markdown file summary or wants to update documentation

## Capabilities

### Read Markdown Files
Use the `read` tool to view file contents:
```
read(file_path: ".skills/markdown-editor.md")
```

### Write Markdown Files
Use the `write` tool to create new or overwrite existing files:
```
write(file_path: "path/to/file.md", content: "# Title\n\nContent...")
```

### Edit Markdown Files
Use the `edit` tool for targeted changes:
```
edit(file_path: "file.md", old_string: "exact text to replace", new_string: "replacement text")
```

### Search Markdown Files
Use glob to find .md files:
```
glob(pattern: "**/*.md")
```

Use grep to search content:
```
grep(pattern: "search term", path: ".", include: "*.md")
```

## Markdown Syntax Reference

### Headings
```markdown
# Heading 1
## Heading 2
### Heading 3
#### Heading 4
```

### Emphasis
```markdown
*italic* or _italic_
**bold** or __bold__
***bold italic***
~~strikethrough~~
```

### Lists
```markdown
- Unordered item
- Another item
  - Nested item
  - Another nested

1. Ordered item
2. Second item
   - Nested with ordered
```

### Links and Images
```markdown
[Link text](https://example.com)
![Alt text](image.png)
```

### Code
```markdown
Inline `code`

```block
code block
```
```

### Tables
```markdown
| Header 1 | Header 2 |
|----------|----------|
| Cell 1   | Cell 2   |
| Cell 3   | Cell 4   |
```

### Blockquotes
```markdown
> This is a blockquote
> It can span multiple lines
```

### Horizontal Rule
```markdown
---
```

## Best Practices
1. Always use `read` before `edit` to see current content
2. Use exact text matching for edits (check for whitespace)
3. Include clear headings and structure for readability
4. Keep documentation files in a `Documentation/` folder
5. Use `glob` with `**/*.md` to find all markdown files in workspace
