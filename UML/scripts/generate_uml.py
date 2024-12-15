import os
import re

# Association detection
def find_associations(src_dir):
    associations = []
    implements_associations = []
    method_associations = []  # To store method-based associations with multiplicity

    # Regular expressions for detecting associations
    method_call_regex = r'(\w+)\.\w+\('  # Detects method calls (e.g., ClassA.Method())
    constructor_injection_regex = r'public\s+(\w+)\s*\((\w+)\s+(\w+)\)'  # Detects constructor injections (e.g., public ClassA(ClassB b))
    method_signature_regex = r'public\s+\w+\s+(\w+)\((.*?)\)'  # Detects method signatures (e.g., public void Method(ClassB b))

    for root, dirs, files in os.walk(src_dir):
        for file in files:
            if file.endswith(".cs"): 
                print(f"Processing file: {file}")  # Debugging line
                with open(os.path.join(root, file), 'r') as f:
                    lines = f.readlines()
                    for line in lines:
                        # Method calls (detecting interactions between classes)
                        match = re.search(method_call_regex, line)
                        if match:
                            class_name = match.group(1)
                            associations.append(('Unknown', class_name))  # Placeholder for method call associations

                        # Constructor injections
                        match = re.search(constructor_injection_regex, line)
                        if match:
                            class_name = match.group(1)
                            dependency_name = match.group(2)
                            associations.append((class_name, dependency_name))  # Constructor injections

                        # Implements (interface implementation)
                        match = re.search(r'class\s+(\w+)\s*:\s*(\w+)', line)
                        if match:
                            class_name = match.group(1)
                            interface_name = match.group(2)
                            implements_associations.append((class_name, interface_name))  # Implements association

                        # Method signatures (for capturing methods and their multiplicity)
                        match = re.search(method_signature_regex, line)
                        if match:
                            class_name = match.group(1)
                            parameters = match.group(2)
                            # We assume if there are commas or collections like List<>, we assign multiplicity
                            if "List" in parameters:
                                multiplicity = "1..*"
                            else:
                                multiplicity = "0..1"  # Default multiplicity if not List or complex
                            method_associations.append((class_name, parameters, multiplicity))  # Store method with multiplicity

    return associations, implements_associations, method_associations

# PUML generation
def generate_uml(associations, implements_associations, method_associations, backend_name):
    uml = f"@startuml {backend_name}\n"
    uml += "left to right direction\n"
    uml += "skinparam linetype ortho\n"
    
    # Add associations to the UML (excluding "Unknown" associations)
    for a, b in associations:
        if a != 'Unknown':  # Exclude 'Unknown' for real associations
            uml += f"{a} -- {b}\n"

    # Add implements associations to the UML (dashed line for "implements")
    for a, b in implements_associations:
        uml += f"{a} ..|> {b}\n"  # UML notation for implements association

    # Add methods and their multiplicity
    for class_name, parameters, multiplicity in method_associations:
        uml += f"{class_name} : {parameters} -> {multiplicity}\n"

    # Group associations
    seen_associations = set()
    uml += "package \"Shared Associations\" {\n"
    for a, b in associations:
        if (a, b) not in seen_associations:
            uml += f"    {a} -- \"shared association\" {b}\n"
            seen_associations.add((a, b))
    uml += "}\n"
    
    uml += "@enduml"
    return uml

# Backends
backend_dirs = [
    "./backend/src", 
    "./employeeBackend/src",  
]

output_dir = "./UML"

# Ensure the output directories exist
backend_output_dir = os.path.join(output_dir, "Backend")
employee_backend_output_dir = os.path.join(output_dir, "BackendEmployee")
os.makedirs(backend_output_dir, exist_ok=True)
os.makedirs(employee_backend_output_dir, exist_ok=True)

# Process each backend
for backend_dir in backend_dirs:
    print(f"Processing {backend_dir}...")
    associations, implements_associations, method_associations = find_associations(backend_dir)
    
    backend_name = os.path.basename(os.path.normpath(backend_dir))
    
    if not associations and not implements_associations and not method_associations:
        print(f"No associations found in {backend_dir}")
        continue
    
    uml_code = generate_uml(associations, implements_associations, method_associations, backend_name)
    
    # Determine the output path based on the backend name
    if "backend" in backend_name.lower():
        uml_output_path = os.path.join(backend_output_dir, f"{backend_name}.puml")
    else:
        uml_output_path = os.path.join(employee_backend_output_dir, f"{backend_name}.puml")
    
    with open(uml_output_path, 'w') as uml_file:
        uml_file.write(uml_code)
    
    print(f"UML diagram generated for {backend_name}.")
