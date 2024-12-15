#!/bin/bash

# Check if the UML directory exists, if not, create it
mkdir -p UML/Backend

# Define output PlantUML file
output="UML/Backend/diagram.puml"

# Start PlantUML syntax for class diagram
echo "@startuml" > $output

# Loop through all C# (.cs) files in the current directory and subdirectories
find . -type f -name "*.cs" | while read file; do
  # Extract class names from the files
  grep -oP 'class \K\w+' "$file" | while read class_name; do
    echo "class $class_name" >> $output
  done

  # Optionally, add more parsing to find relationships (e.g., inheritance or interfaces)
  grep -oP 'class \w+ : \w+' "$file" | while read relationship; do
    # This handles inheritance, for example: class A : B
    class1=$(echo $relationship | cut -d' ' -f1)
    class2=$(echo $relationship | cut -d' ' -f3)
    echo "$class1 -|> $class2" >> $output
  done

  # Parse methods and link them to classes (optional)
  grep -oP 'class \w+' "$file" | while read class_name; do
    methods=$(grep -oP "(public|private|protected|internal|void|static|sealed|\w+)\s+\w+\s+\w+" "$file" | grep -oP '\w+\s*\(')
    for method in $methods; do
      echo "$class_name : $method" >> $output
    done
  done
done

# End PlantUML syntax
echo "@enduml" >> $output

# Print message when done
echo "PlantUML file generated at $output"
