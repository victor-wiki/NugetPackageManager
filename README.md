# NugetPackageManager
A visual tool to use dotnet nuget command to push and delete nuget package.

## Use
1. Specify a key file, its content seems like the format "KeyName:KeyValue":
~~~
KeyName1:xxxxxxxxxxxxxxxxxxxxxxxxxxxxx
KeyName2:yyyyyyyyyyyyyyyyyyyyyyyyyyyyy
~~~
2. Config pattern for Key, the pattern should be same as nuget api key pattern.
3. Select a package file, it can auto detect the key and package version from the file name via matching the configued key patterns.

## UI - Main
![Main](https://github.com/victor-wiki/StaticResources/blob/master/StaticResources/images/projs/NugetPackageManager/Main.png?raw=true&rid=1)

## UI - Key Setting
![Key Setting](https://github.com/victor-wiki/StaticResources/blob/master/StaticResources/images/projs/NugetPackageManager/KeySetting.png?raw=true&rid=1)
