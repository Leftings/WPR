import os

# Associetie detectie
def find_associations(src_dir):
    associations = []
    for root, dirs, files in os.walk(src_dir):
        for file in files:
            if file.endswith(".cs"): 
                with open(os.path.join(root, file), 'r') as f:
                    lines = f.readlines()
                    for line in lines:
                        if "uses" in line:
                            classes = line.split("uses")
                            associations.append((classes[0].strip(), classes[1].strip()))
    return associations

# PUML
def generate_uml(associations, backend_name):
    uml = f"@startuml {backend_name}\n"
    uml += "left to right direction\n"
    uml += "skinparam linetype ortho\n"
    
    # Associeties
    for a, b in associations:
        association_name = f"\"{a} to {b}\""
        uml += f"{a} -- {association_name} {b}\n"

    # Groepering associaties
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

for backend_dir in backend_dirs:
    print(f"Processing {backend_dir}...")
    associations = find_associations(backend_dir)
    backend_name = backend_dir.split('/')[-2]
    
    uml_code = generate_uml(associations, backend_name)
    
    uml_output_path = f"./UML/{backend_name}.puml"
    with open(uml_output_path, 'w') as uml_file:
        uml_file.write(uml_code)
    
    print(f"UML diagram generated for {backend_name}.")

