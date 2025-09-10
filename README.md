# 💸 Municipal Issue Reporting Web Application

## 📝 DESCRIPTION  
This project is a web application built with ASP.NET Core MVC, C#, JavaScript, and Leaflet.js. It allows users to report municipal issues, view reports on a map, and track the verification and resolution process. The app provides a modern, interactive experience with pop-up confirmation modals, a multi-step form, and ward-level statistics.

## WHAT IS THE PURPOSE OF THE APP?  
This application was built to empower residents to easily report municipal issues, increase transparency in issue resolution, and enable administrators to monitor, verify, and resolve reports efficiently. By providing real-time maps, progress tracking, and user-friendly reporting tools, the app encourages civic engagement and helps improve service delivery.

---

## 💡 DESIGN OVERVIEW  

### **UI/UX Design**  
- Multi-step reporting form with progress bar  
- Interactive map showing all reports with status color-coding  
- Modal pop-up for successful submissions  
- Styled Back, Next, and Submit buttons  
- Accessible design with proper contrast and touch targets  
- Responsive layout for desktop and mobile  

### **🎨 Color Scheme**  
- Back button: White with black border  
- Next button: Blue (#27A9F5)  
- Submit button: Green (#43F773)  
---

## 💿 BACKEND ARCHITECTURE  

**Data Management**  
- Reports represented by `IssueReport` model: ReferenceNumber, Location, Category, Description, Attachments, CreatedUtc  
- Attachments stored in `Attachment` model with FileName, ContentType, StoredPath, and LengthBytes  
- Multi-step submission uses `IssueReportCreateRequest` to handle user input and file queue  

**Custom Data Structures**  
- `DoublyLinkedList<T>` to manage attachments  
- `Queue<T>` for handling file uploads in FIFO order  
- Both structures are generic and lightweight, built for in-memory operations  

**Map Integration (Leaflet.js)**  
- OpenStreetMap tiles for geolocation  
- Status color-coded pins: red, yellow, green  
- Pop-ups for “Still a Problem” or “Resolved” confirmations  

**Form Handling**  
- Multi-step form validation and progress tracking  
- File upload support with queue management  
- Success modal with reference number and copy-to-clipboard  

**Security Features**  
- Input validation on all fields  
- Error handling with user-friendly messages  

---

## 👩‍💻 GETTING STARTED  

1. Within the repository, click on the "<> Code" drop down on the far right next to the "Go to file" and "+" buttons.
2. On the Local tab, click on the last option: "Download ZIP".
3. Once the zip file has downloaded, open your local file explorer.
4. Go to your Downloads.
5. Open the "ST10251981_PROG6212_Part1.zip" folder, should be most recent in Downloads.
6. Open the "ST10251981_PROG6212_Part1" folder, this folder is not a zip.
7. Open the CityPulse.sln file.
8. The project should begin loading.
9. On the top in the middle, double click the http button.
10. The program will compile and you may use the program.

## 👾 TECHNOLOGIES USED
ASP.NET Core MVC, C#, JavaScript, Leaflet.js, Bootstrap 5, HTML5, CSS3

## 🎲 FEATURES

### Users can:

1. Report municipal issues via a multi-step form
2. Select location from search suggestions or map
3. Upload images/documents with reports
4. 4.Track report progress in real-time
5.Verify reports as “Still a Problem” or “Resolved”
6.View ward-level statistics and interactive map pins

### Additional Features:

- Success modal pop-up with reference number and copy functionality
- Styled buttons for clear navigation (Back, Next, Submit)
- Responsive design for mobile and desktop
- Custom in-memory structures (DoublyLinkedList, Queue) for attachments and uploads


## 📺 YOUTUBE LINK:
