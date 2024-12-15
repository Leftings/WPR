#!/bin/bash

# Input directory: the first argument is the source directory
input_dir=$1

# Output file: the second argument is the path to the .puml file
output_file=$2

# Check if the input directory exists
if [ ! -d "$input_dir" ]; then
    echo "Error: Input directory $input_dir does not exist."
    exit 1
fi

# Start the UML content with the PlantUML header
echo "@startuml" > "$output_file"

# Loop through all C# files in the source directory and subdirectories
find "$input_dir" -type f -name "*.cs" | while read -r cs_file; do
    # Read the C# file and extract class names and methods
    while IFS= read -r line; do
        # Match class definitions: e.g., class ClassName
        if [[ "$line" =~ class\ ([a-zA-Z_][a-zA-Z0-9_]*) ]]; then
            class_name="${BASH_REMATCH[1]}"
            echo "class $class_name" >> "$output_file"
        fi

        # Match method definitions: e.g., public void MethodName()
        if [[ "$line" =~ [[:space:]]*(public|private|protected|internal)[[:space:]]+(static|void|[a-zA-Z0-9_]+)[[:space:]]+([a-zA-Z_][a-zA-Z0-9_]*)\([[:space:]]*\) ]]; then
            method_name="${BASH_REMATCH[3]}"
            echo "$class_name : +void $method_name()" >> "$output_file"
        fi
    done < "$cs_file"
done

# End the UML content with the PlantUML footer
echo "@enduml" >> "$output_file"

echo "Generated UML file at $output_file"
