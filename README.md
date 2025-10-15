# 💸 Municipal Issue Reporting Web Application

## 📝 DESCRIPTION  
This project is a web application built with ASP.NET Core MVC, C#, JavaScript, and Leaflet.js. The system is a efficient and user-friendly platform for citizens to access and request various municipal services. 

## WHAT IS THE PURPOSE OF THE APP?  
This application was built to empower residents to easily report municipal issues, increase transparency in issue resolution, and later on enable administrators to monitor, verify, and resolve reports efficiently. By providing real-time maps, progress tracking, and user-friendly reporting tools, the app encourages engagement and helps improve service delivery.

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
4. Track report progress in real-time
5. Verify reports as “Still a Problem” or “Resolved”
6. View ward-level statistics and interactive map pins
7. Search for announcements/news using keywords from titles or descriptions
8. Filter announcements by category and date range
9. View a variety of announcements
10. Admins can login and create announcements 

### Additional Features:

- Success modal pop-up with reference number and copy functionality
- Styled buttons for clear navigation (Back, Next, Submit)
- Responsive design for mobile and desktop
- Custom in-memory structures (DoublyLinkedList, Queue) for attachments and uploads

## 🧩 Data Structures Used in CityPulse (with Reasons)

### 🔧 Custom Data Structures (in Domain.cs)

**DoublyLinkedList<T>**

- **Used for:** Attachments in IssueReport
- **Why:** Allows quick adding/removing files at both ends. You can move forwards and backwards easily without resizing like an array.
- **Also used for:** Location suggestions seed data — shows our own custom structure and lets us loop through stored locations smoothly.

**Queue<T> (Custom)**

- **Used for:** File upload queue in IssueReportCreateRequest
- **Why:** Works in a FIFO (First-In-First-Out) way, which fits how uploads are processed — the first file added is the first one uploaded.
- **Also used for:** Returning location suggestions in the order they appear.

### ⚙️ Built-in .NET Data Structures (in AnnouncementService)

**SortedDictionary<DateTime, List<Announcement>>**

- **Used for:** Main announcement storage.
- **Why:** Keeps announcements sorted by date automatically and allows fast date searches (O(log n)).

**Dictionary<AnnouncementCategory, List<Announcement>>**

- **Used for:** Category-based lookup.
- **Why:** Makes it quick (O(1)) to get announcements in a certain category like Events or Notices.

**Dictionary<Guid, Announcement>**

- **Used for:** Finding announcements by ID.
- **Why:** Fast O(1) lookup for specific announcements without searching through all items.

**Dictionary<string, HashSet<Guid>>**

- **Used for:** Text search index.
- **Why:** Maps keywords to announcement IDs for quick searching. You can find results fast by intersecting sets of matching words.

**HashSet<string>**

- **Used for:** Storing unique categories.
- **Why:** Automatically avoids duplicates and allows instant O(1) checks for category existence.

**HashSet<DateTime>**

- **Used for:** Unique announcement dates.
- **Why:** Ensures no duplicate dates and helps create a clean list for date filters or calendar views.

**PriorityQueue<Announcement, int>**

- **Used for:** Featured or high-priority announcements.
- **Why:** Keeps announcements sorted by priority (Critical, High, etc.) so the most important ones show first.

**Stack<Announcement>**

- **Used for:** Tracking recently viewed announcements.
- **Why:** LIFO (Last-In-First-Out) order matches how users revisit their last viewed announcements.

**Queue<Announcement>**

- **Used for:** Pending announcements waiting for admin approval.
- **Why:** FIFO order ensures fairness — the first submitted is reviewed first.

### 📋 Standard Collections

**List<T>**

- **Used for:** Return types and temporary collections.
- **Why:** Easy to use, supports LINQ, allows indexing, and resizes automatically. It's great for MVC views.

---

## 📺 YOUTUBE LINK:
