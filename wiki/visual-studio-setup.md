Visual Studio is arguably the best .NET IDE out there because it provides so many utilities and works seamlessly with .NET. Since Godot uses .NET, it makes sense to take advantage of the tools that VS provides. Luckily, we can easily link Godot with VS.

1. Set Visual Studio as the default editor.
	a. Open Godot
	b. Open a project
	c. Select the "Editor" tab > "Editor Settings"
	d. Scroll to "Dotnet" and select "Editor"
	e. For "External Editor" select "Visual Studio".
	f. For custom Exec Path" select the Visual Studio executable. This is found at `"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe"` if you place VS in the default location.

2. Download Godot Support
	a. From https://github.com/godotengine/godot-csharp-visualstudio/releases, download the .vsix v2.0.0 file and run.

3. Create a Script and Open in VS
	a. VS should show the solution and all your scripts in the solution explorer. If not, there might an install panel telling you what you need. Hopefully there is because there was for me and that got things working. I don't remember what it installed, though.

# Sources

Reddit Post: https://www.reddit.com/r/GodotCSharp/comments/xgpqfh/oc_rundebug_godot4_c_projects_from_visual_studio/?onetap_auto=true
Video Tutorial: https://www.youtube.com/watch?v=jSpstgUy7fc
