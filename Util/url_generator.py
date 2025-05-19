def create_url_file():
    # Open file in write mode
    with open('urls.txt', 'w') as file:
        # Generate 2000 URLs (0 to 1999)
        for i in range(2000):
            url = f"http://localhost/MacOS/test_{i}\n"
            file.write(url)

    print("File 'urls.txt' has been created successfully!")

# Run the function
create_url_file()
