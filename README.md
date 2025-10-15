# 💸 Municipal Issue Reporting Web Application

## 📝 DESCRIPTION  
This project is a web application built with ASP.NET Core MVC, C#, JavaScript, and Leaflet.js. The system is a efficient and user-friendly platform for citizens to access and request various municipal services. 

---  

### **🎨 Color Scheme**  

**Navigation Buttons:**
- Back button: White with black border
- Next button: Blue (#27A9F5)
- Submit button: Green (#43F773)

**Announcement Categories:**
- Events: Cyan/Info Blue (#17a2b8)
- Service Updates: Green (#28a745)
- Announcements: Primary Blue (#007bff)
- Notices: Orange (#fd7e14)
- Programs: Gray (#6c757d)
- Emergency: Red (#dc3545)

**Status Indicators:**
- Success/Confirmation: Green
- Information: Blue
- Warning: Yellow/Orange
- Error/Critical: Red
- Featured Items: Red border with gradient background
---
 

---

## 👩‍💻 GETTING STARTED  

1. Within the repository, click on the "<> Code" drop down on the far right next to the "Go to file" and "+" buttons.
2. On the Local tab, click on the last option: "Download ZIP".
3. Once the zip file has downloaded, open your local file explorer.
4. Go to your Downloads.
5. Open the "ST10251981_PROG6212_POE.zip" folder, should be most recent in Downloads.
6. Open the "ST10251981_PROG6212_POE" folder, this folder is not a zip.
7. Open the CityPulse.sln file.
8. The project should begin loading.
9. On the top in the middle, double click the https button.
10. The program will compile and you may use the program.

## 👾 TECHNOLOGIES USED
ASP.NET Core MVC, C#, JavaScript, Leaflet.js, Bootstrap 5, HTML5, CSS3

## 🎲 FEATURES

### Users can:

1. Report municipal issues via a multi-step form
2. Select location from search suggestions or map
3. Upload images/documents with reports
4. Track report progress in real-time
5. Verify reports as "Still a Problem" or "Resolved"
6. View ward-level statistics and interactive map pins
7. Create accounts and login for personalized recommendations
8. Search for announcements/news using keywords from titles or descriptions
9. Filter announcements by category and date range
10. View personalized recommendations based on their interests
11. Add announcements to their interests for better recommendations
12. Admins can login and create announcements


### Admin log in details
- username: admin
- password: Admin@123!

## 🎯 Personalized Recommendation System

The application features an intelligent recommendation engine that learns from user behavior to suggest relevant announcements and events. When users create an account and log in, the system automatically tracks their search patterns and category preferences to build a personalized profile.

**How It Works:**

Every time you search for something like "water" or "community," the system remembers those terms. When you filter by a specific category like "Events" or "Programs," that preference gets saved with extra weight. Even just clicking on announcements helps the system learn what kind of content you're interested in. All of this happens automatically in the background without any extra effort from you.

**The Recommendation Algorithm:**

The system uses a smart scoring algorithm to rank announcements based on your preferences. If you've clicked on Events five times and Programs twice, Event announcements get higher scores and appear more often in your recommendations. Search terms you've used also boost matching announcements, so searching for "water" multiple times means you'll see more water-related content. The algorithm combines category preferences, search history, trending content, and upcoming events to create a personalized feed just for you.

**Data Structures Powering Recommendations:**

I use Dictionary structures to track search history (storing your last 20 searches) and category preferences (counting how many times you've interacted with each category). A SortedDictionary keeps track of trending announcements by view count, automatically organizing them so popular content surfaces quickly. HashSets prevent showing you duplicate recommendations and make it fast to check if you've already seen something.


## 🧩 Data Structures Used in CityPulse

### 🔧 Custom Data Structures

**DoublyLinkedList<T>**

We use this to store attachments in issue reports since it's easy to add or remove files from either end without having to resize anything (like you would with arrays). It also lets you move forward and backward through the list easily. We also used it for location suggestion seed data so we can loop through stored locations smoothly while showing off our own custom data structure.

**Queue<T>**

This is used to manage the file upload queue when creating issue reports. It works in a First-In-First-Out way — so the first file added is the first one uploaded, which just makes sense for uploads. We also use it to return location suggestions in the same order they appear, so users get results in a natural flow.

### ⚙️ Built-in .NET Data Structures

**SortedDictionary<DateTime, List<Announcement>>**

This one stores announcements sorted automatically by date. It's super handy because it makes finding announcements for a specific day or range really fast and organized.

**Dictionary<AnnouncementCategory, List<Announcement>>**

Used for category-based lookups, so when we need to grab all announcements under something like "Events" or "Notices," we can do it instantly without searching through everything manually.

**Dictionary<Guid, Announcement>**

This makes it easy to find a specific announcement using its unique ID. Since it's O(1), lookups are super fast — perfect when we only need one item.

**Dictionary<string, HashSet<Guid>>**

Used for the text search feature. It basically maps keywords to announcement IDs so we can quickly find matches without scanning every single announcement. Makes searches a lot faster and more efficient.

**HashSet<string>**

We use this to store all the announcement categories. It automatically stops duplicates and lets us check if a category exists right away. It's great for things like category filters or dropdowns.

**HashSet<DateTime>**

This one keeps track of all the unique dates that have announcements. It prevents duplicates and helps us build clean date filters or calendar views — so users only see dates that actually have announcements.

**PriorityQueue<Announcement, int>**

Handles high-priority announcements. It keeps them sorted by priority level automatically, so important stuff (like critical updates) always shows up first.

**Stack<Announcement>**

Used for recently viewed announcements. Since it works in a Last-In-First-Out way, it fits perfectly for "recently viewed" features — the last thing you looked at is always on top.

**Queue<Announcement>**

Stores announcements that are waiting for admin approval. It's fair because it works First-In-First-Out — the first one submitted gets reviewed first.

**List<T>**

We use lists everywhere for temporary collections, returning data, or passing stuff to MVC views. They're flexible, easy to use with LINQ, and resize automatically — so they just make sense for most general collection needs.

---

## 📺 YOUTUBE LINK: https://youtu.be/G_S5Ybnx8RE 
