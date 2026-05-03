"""
Read and store file contents.

Opens a file in read mode and loads its entire content into memory.
Uses context manager to ensure proper file closure.
"""
with open("file.txt", "r") as file:
    # Read the file and print the content
    contents = file.read()
    print("```")
    print(contents)
    print("```")

