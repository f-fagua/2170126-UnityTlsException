# Read the input file and convert comma-separated strings to line-separated strings
def process_file(input_file, output_file):
    try:
        # Read the input file
        with open(input_file, 'r') as f:
            content = f.read()
        
        # Split the content by commas
        strings = content.split(',')
        
        # Remove "@@@" and subsequent characters from each string
        processed_strings = [s.split('@@@')[0].strip() for s in strings]
        
        # Write the processed strings to the output file, one per line
        with open(output_file, 'w') as f:
            for string in processed_strings:
                f.write(string + '\n')
                
        print(f"Processing complete. Results saved to {output_file}")
        
    except FileNotFoundError:
        print(f"Error: The file {input_file} was not found.")
    except Exception as e:
        print(f"An error occurred: {str(e)}")

# Example usage
input_file = './AssetBundleURIs.txt'  # Replace with your input file name
output_file = './real_urls.txt'  # Replace with your desired output file name

process_file(input_file, output_file)
