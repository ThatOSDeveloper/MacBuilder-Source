# MacBuilder

This project is open for contributions from the community to help improve its features. Please read the following guidelines carefully before using or contributing to this repository.

## Usage Guidelines

1. **No Commercial Use:**  
   This code is provided for educational, collaborative, and personal use only. You are **not allowed** to sell, monetize, or use this project or its code in any commercial manner.

2. **No Unauthorized Redistribution:**  
   This project and its contents may **only** be published or distributed globally by **Kivie** (the original author). Any unauthorized publishing or redistribution is strictly prohibited.

3. **Contributions:**  
   - Contributions are welcome! If you'd like to contribute, please open a pull request or submit issues for discussion.  
   - By contributing to this repository, you agree that your contributions may be used as part of this project under the terms outlined here.

4. **Attribution:**  
   If you use any part of this code in personal or collaborative projects, you must give appropriate credit to the author (**Kivie**). 

## Disclaimer

This project is provided "as-is," without any warranty or guarantee of functionality or support. The author is not responsible for any issues or damages that arise from the use of this code.

Thank you for respecting these guidelines. Let's collaborate to make something amazing together!

## License
- For further inquiries, please contact me directly.

## How to Compile

### Requirements
- **Visual Studio 2022** (Preferably the latest version).  
- **Windows 10.0.22621.0 SDK**  
  - [Download the SDK here](https://developer.microsoft.com/en-us/windows/downloads/windows-10-sdk/).  

### Steps
1. **Download the Repository**  
   Clone or download the repository to your local machine.  

2. **Open the Solution**  
   Open the project solution file (`.sln`) in Visual Studio 2022.  

3. **Set Build Configuration**  
   - Set the **Build Configuration** to **Debug**.  
   - Set the **Platform** to **x64**.  

4. **Build the Project**  
   - In Visual Studio, go to **Build** > **Build Solution**.  

5. **You're Good to Go!**  

### Troubleshooting
For any issues, refer to the **[Issues](../../issues)** tab in this repository or feel free to open a new issue.  

## Workflow

MacBuilder currently operates as follows:

1. **Hardware Analysis:**  
   The **MHC** tool dumps a JSON file containing details about your hardware.  

2. **USB Selection:**  
   Once the hardware analysis is complete, you select a USB drive from the available options.  

3. **USB Formatting:**  
   The selected USB is formatted to prepare it for use.  

4. **macOS Compatibility Check:**  
   Based on the hardware analysis, MacBuilder provides a list of compatible macOS versions for your specific hardware.  

5. **OpenCore Installation:**  
   - The selected macOS version is used to extract the base OS and OpenCore files onto the USB.  
   - The program automatically organizes and cleans up the extracted files to ensure a streamlined setup.  

## Roadmap

### Planned Features
- **Easy-to-use and Modify UI**  
  A user-friendly interface that allows easy customization and changes of kexts, ACPI, Drivers, etc.

- **Automatic Kexts, ACPI, Drivers Download**  
  Automatically download Kexts, ACPI, Drivers, etc based on the hardware.

- **Built-in Installer**  
  Download Kexts, ACPI, Drivers, etc directly from MacBuilder.

- **OCValidator Integration**  
  Integrate OpenCore Validator tool to validate EFI.

- **Boot-Picker Customization**  
  Allow customization of the boot-picker screen, including themes, options, and additional features.

- **Backup and Restore**  
  Enable the ability to back up and restore EFI.

- **Multilingual Support**  
  Add support for multiple languages.

- **Virtual USB Drive**  
  Implement a feature to use a portion of the system's storage as a virtual USB drive, simulating a physical USB stick.

---

## TODO

### Features to be Developed or Improved
- **Improve UI**  
  Improve the user interface to make it more responsive, and visually appealing.

- **Improve Code**  
  Improve and optimize the codebase for better readability.

- **Code Cleanups**  
  Address code quality issues, remove unnecessary code.

- **Better Hardware Checks**  
  Improve the hardware compatibility checks.

---

# Discord Server
Join the official [Discord Server](https://discord.gg/7FhHhjm9uu)

---

# Support The Creator
If you'd like to support me, consider donating here: [Buy me a coffee.](https://buymeacoffee.com/kiviedev)

---
The project is actively being developed, and community contributions are welcome to help improve its features and functionality.

