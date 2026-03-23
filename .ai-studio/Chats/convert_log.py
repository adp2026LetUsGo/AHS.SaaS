import json
import os

def convert_json_to_md(input_file, output_file):
    if not os.path.exists(input_file):
        print(f"Error: Input file {input_file} not found.")
        return

    with open(input_file, 'r', encoding='utf-8') as f:
        data = json.load(f)

    chunks = data.get('chunkedPrompt', {}).get('chunks', [])
    md_content = "# MISSION CHRONICLES V1\n\n"

    for chunk in chunks:
        role = chunk.get('role')
        text = chunk.get('text', '')
        
        # Determine header based on role
        if role == 'user':
            md_content += "# User\n\n"
        elif role == 'model':
            md_content += "# Assistant\n\n"
        else:
            # Skip unknown roles if any
            continue

        # Handle text content
        # Note: The JSON seems to have code blocks already formatted with ``` 
        # but sometimes it uses \n for newlines.
        md_content += text.strip() + "\n\n"

        # Check for parts if text is empty or to complement
        parts = chunk.get('parts', [])
        for part in parts:
            part_text = part.get('text', '')
            if part_text and part_text.strip() not in md_content:
                md_content += part_text.strip() + "\n\n"

    # Ensure target directory exists
    os.makedirs(os.path.dirname(output_file), exist_ok=True)

    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(md_content)
    
    print(f"Successfully converted {input_file} to {output_file}")

if __name__ == "__main__":
    input_path = r"c:\Users\armando\Documents\_AHS\projects\AHS.SaaS\AI Studio\Chats\Gestionando AI Studio"
    output_path = r"c:\Users\armando\Documents\_AHS\projects\AHS.SaaS\AI Studio\Chats\Gestionando AI Studio\MISSION_CHRONICLES_V1.md"
    convert_json_to_md(input_path, output_path)
