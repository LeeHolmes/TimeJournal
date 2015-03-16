# TimeJournal
Time Journal application to help you record and track your time.

In your work life, it is incredibly helpful to know how you spend your time. Personally, it greatly
helps improve your estimation skills: did you really spend as much time on the project as you thought you would?
Professionally, it helps you remember important events for a given time period. For example, pulling status reports
together for a manager, or reviewing your year’s accomplishments in preparation for your yearly review.

I’ve been using an Time Journal in one way or the other for many years now, and definitely consider it a core tool / technique. 

Time Journal helps you analyze your time by infrequently asking the simple question: “What are you doing?”. Just place it in your
startup folder and the rest is easy.

How it Works 
------------

Time Journal follows the same principles as a traditional software sampling profiler, but instead samples humans.
By randomly recording your current task, Time Journal lets you analyze your answers as a fairly faithful proxy for
how you actually spent your time. If 20% of your answers were “Status Meeting,” then you spent close to 20% of
your time in status meetings. 

An Alternative to Sampling 
--------------------------

An alternative to the sampling approach is an instrumentation approach: faithfully recording your transition between tasks.
Time Journal avoids this design, since asking humans to faithfully record transitions between tasks is enormously error-prone.
For example, you might not log a task transition for a task that you consider inconsequential (for example, “Checking email”,) when
in fact that task may account for a significant portion of your day. Some software attempts to address the human element by tracking window
titles, but the level of data captured by window titles often does not map well to the task they support.

Using Time Journal 
------------------

Time Journal is a WPF application.

Once launched, Time Journal sits in the background. Once in awhile (randomly, between 5 and 25 minutes,) it asks you the question, “What are you doing?”
It stores your previous answers in a list until you exit the program, which lets you easily re-use your answers to previous questions. It re-populates
this list from the last week of activities whenever you start the program.

When you press OK, it adds your answer (along with the current window title) to a file in “My Documents\TimeJournal” – one file per week. The file is
named to correspond to the date on the first day of the week. 

If you don’t answer within four minutes, it dismisses the dialog and checks your Outlook calendar. If you are in a meeting, it records the title
of that meeting. If you aren’t in a meeting, it records nothing. This lets you keep the Time Journal running when you go home for the day without
polluting your journal files. 

Slicing and Dicing 
------------------

The Time Journal records its output as a simple CSV file. Knowing that, you can slice and dice results to your heart’s content.
For example, to easily get a summary of your week from PowerShell:
                                                                  
```																  
PS >Import-Csv temp.csv | Group Activity | Sort -Descending Count          
                                                                           
Count Name                      Group                                      
----- ----                      -----                                      
   23 Hubble Space Telecsope... {@{Date=5/20/2009 8:24:19 AM; WindowTi... 
    8 Meeting: Design review    {@{Date=5/20/2009 1:10:21 PM; WindowTi... 
    5 Meeting: Team meeting     {@{Date=5/20/2009 3:10:20 PM; WindowTi... 
    4 Email                     {@{Date=5/20/2009 8:04:26 AM; WindowTi... 
    3 Scripting games           {@{Date=5/19/2009 6:09:16 PM; WindowTi... 
```

To count how many hours you spent on a task, simply divide by four.