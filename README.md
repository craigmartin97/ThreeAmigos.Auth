ThAmCo Auth

The ThAmCo Auth software application performs the following functionality:
1) CRUD's users to a database
2) Generates access tokens for user usage
3) The system is an OAuth implementation with IdentityServer4

Getting Started

To get the latest copy of the source code onto your local machine it is recommended you 
install GIT and clone a new copy of the source code from master.

Prereqisites
1) Visual Studio (Preferably 207 of later)
2) SQL Management Studio 2017, with a local server installed.
3) GIT

Deployment
To deploy this to the live system the, complete the following steps:
1) Make changes and add a commit to the master branch
2) Once happy, push the changes to origin master and the new deployment is then live

NOTE: Azure Pipelines has been configured for this application using docker support.

Built With
Visual Studio 2019
SQL Management Studio 2017
GIT 2.24.1

Authors
Craig Martin, Teesside University Student

License
This project is licensed under the GNU General Public License (GPL)