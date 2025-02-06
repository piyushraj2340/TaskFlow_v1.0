# Task Monitoring Applications 

Todo: 
1. Implement the error handling for the developer mode : redirect.... for both.. || have the both error throwing mode for json and view...
2. design the error page beautiful 
8. implement the add notes with the time stamp and having the roadmap or planning....
	1. ISpecification use of this to apply the filter....
3. Implement the todo page... such that get the information about the daily routine ... working on...✅
4. Implement the logic for the having an instance of the task tha have an repeat daily or weekly ✅
5. analysis the data simple only such as completed task count, goal achieved count, success ratio, etc....✅
6. implement the time taken to complete the task manual latter implement the tracker.... or having both...
7. filter the data like in the active row only show the active, complete only show complete... ✅
10. implement the pop-over when we attamps to delete the data in the row to confirm, you want to delete or not. ✅
	11. Authentication using the identity asp.net or both... to learn ... ✅ using the identity
	
	1. 1. Implement the active and completed goal  same goes to the task ✅
	1. add endedonDate in both task, goal.. modify if the status changes to the end then make the datetime to the current so that next moment it goes to end and also update the status...✅
	1. implement the redirect logic from the same page that have been called for the create task and edit task.. ✅
	1. calculations is done using the mvc model -- productivity... using the service... ✅
	1. all the calculations goes through the stored procedured.... 

	for the mvc that direcly render like in the details : generate the status in more better ways like: delete have red color..✅ 
	1. 
	1. Add the date like: CompletedOn, EndedOn, StartedOn, EndingOn, etc... in the next version of projects...✅
	1. Add the descriptions, priority, that have been added today... on the model class ✅

	        [Range(minimum:0,maximum:100)]
        public int PerformanceScale { get; set; } // this help me to calculate the actual productivity of the task...

		if needed add this also implemnt the custome productivity by the user 

	
		check if we Add validations in the controller 

		add the logic where user can eaisly review there progress on the daily, weekly, monthly, and yearly basis 

		improve the logic of repeat just like the google task....

		Test the logic for the task instance in the todo 



		bugs list 
	1. sorting on the datatable....
	2. review the calculations of goal,task,and todos.... 

# Add UserBased control in service, repository if need....✅
# Add username while register user  ✅
# add the task, goal, todo, productivity controller to the authorize ✅
# add the model error in the mvc patter using the partial view  ✅
# add the icon in the navigation bar nav link 
# add the border shadow in the model validations in the model modelErrorPartialView ✅
# learn how to use the social login using the identity model 
# implement the admin login to view activity of the user, such as userVerifications, block user, verify user, or create new user, add site settings such as upload logo, use the default theme, see the logs, etc.... 
# implement the redirect logic in the login page....


# need to test the updateExecuteAsync if not work implement the saveChangesAsync()
# test if the goal.user.id work or not if not use the another LINQ based approach used the include✅
# redirect logic if we add, update, task as we have made to redirect to back page but if we have directly come to that page so where to redirect....

# test the todo to the and add the todo in repository layer 
# label in the task appears to be wrong fix it as low display as high and high will display as low.....
# test all the label properly for goal, task, todo.....
	

## Productivity calculations (Use SignalR - If Needed!)

# Todo Productivity: -> calculate daily productivity -> total Task and completed task and get the percentage 
# Task Productivity: currently calculated as Total Task = (Completed Task + Ended Task ) and completed task  and get the percentage 
# Goal Productivity: currently calculated as Total Goal = (Completed Goal + Ended Goal ) and completed goal  and get the percentage 

# Need to calculate (((completed)/((running + completed + end) as total)) * 100) 

# Calculate Growth by Previous Week : (Get the Average productivity of 7 days) and (Get the Average productivity of 7 days to 14 days) and then calculate the productivity accordingly 


```

formula - (((Presend week productivity - past week productivity ) / past week productivity) * 100)

```

# Overall Productivity: (total productivity) / (total active day + missing day)

# Productivity of selected goal -> ((completed task with related to that goal / sum(running task, completed task, ended task)) * 100)


### Features that need to add

# Add the graph calculation 
# Add Streak to auto complete task or goal
# Add the logic to make the todo complete or undo for the previous day 
# Add the options to provide the custom productivity for each day with Notes...
# Add Note in the task and goal and make an UI that let you see your progress and what need to change with time 
# enhance the repeat logic for task -- see the reference for google task
# Implement the admin control - such as - site settings -- update logo, allow verify user, user can limit to add the goal, task, etc..., user control - block,change password, verify user, etc....
# Implement in the Account controller social login, forget password, change password, two-factor authentication, etc...


### Todo 
# Add SearchGoalBySearchQuery in service layer 
# implement the sp to create the task by adding the goal
# Add the Task with goal through the sp 
# need to test the logic of debouncing in the task create view....

# validate the add-goal in the goal list and selectedgoal list after the selected goals 

# fetch Goal Associated while edit or post create render in the AddTask or editTask 


# GoalName must be unique for the users 
# TaskName must be unique for the users, goal 
	##	-- like if the user has created a task with goal therefore if the user re-create a task with the same name with goal will not allowed
	##  -- similar if the user has create a taks without goal will act as independent and can created with same same with other goal 
	##  -- suggest the name if the taskName already taken with like task 1 already exit and if we write task 1 and selct with the goal 1 
		###  --- suggestiong like task 1 with goal 1 add explictily and give the suggestion name.....
--- use the sp for the above functionality... ---

# GoalEndDate - TaskEndDate does it need the dateTime or just need the date : ans: need datetime but time end to mid-night like : 23:59:59 
# if goal or task mark as completed and then change the status to running: need to add the notes before move to running....
# also add the notes automatically when status changes or task completed, etc.... and have tags manual notes or log notes....

# Repository accept the parameter of entity and return entity  ✅
# Service accept the parameter of dto and return dto ✅
# Controller accept the DTO or ViewModel and retrn the DTO for api and return mvc for viewmodels ✅


# map the dto and viewmodel so that it can transform the data.... ✅

# remove the status 'delete' from and create one column that will handle te isDelete and deletedOn.... ✅

# implement queue to change the goalStatus as ended to act as background processing.....  by using the entity framework remove the stored procedure....

# Implement the logic if we change the status from 'ended' to 'running' or 'ended' to 'completed' must need to add the notes before changing the status without notes are not allowed...


# Reduce the round trip to the db in the delete and update operations.... either by using the ef or stored procedure in single round-trip
# implement the GetAllTasksWithStatusByGoalId in the goal details page (repo, service completed need to implement the controller.... need testing...)
# implement the GetAllGoalsWithStatusByTaskId in the task details page


# in the home index page productivity calculation not working in the task

# we have encounter an error tasks model is not maped with the table in the db find the issue in this model....

# all the mapper and logger dependency will be in the service layers.... 

# goal have only end date not time.... expelicetly do in service layers....
 
# EndedGoal, EndedTask, should be handeled by the in-memory queus not by the stored procedured. so remove the sp after this....
# handel the update functionality in only 1 round trip....



# update the frontend logic such as : endDate validations and all...
# edit task with goal not saving goals 
# see how the task end date saving date and time 


# when I implemented the Iunit of work patter then addtaskwithgoal will be added using the entity...
  
  #TODO PAGE NOW WOKING NOW BUT HAS ISSUE WITH CREATING THE INSTANCE OF THE TASK 

  CREATEING THE MULTIPLE TODO WITH THE SAME DATE 

  TODO LINK TO TASK IS NOT OPENING SOME 0 IDS 

  SEARCH GOAL ON THE TASK CREATION WILL NOT HIDE IF OTHER FIELD IS SELECTED 

  PRIORITY TAG APPERS WRONGS 

  STILL NOT GETTIG THE GOAL DETAILS ON EDITING 

  Add validation in the sp to only active goal is added to the task

  fix: end time for goal,task,todo at :23:59:59

 Scheduled move to running.... like planning
 
 add the components on planning on goal and task both...

 add deleted and ended todos 



 ### Features : notes..
# Introduce the concept of the category, labels, or tags, etc dynamic....
# configure the sticky notes models such as time,stickytilldate,timerange,closecount,etc....
#