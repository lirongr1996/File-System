# File-System
In this project, I implement file system in c#

File System structure :

![image](https://user-images.githubusercontent.com/81233165/139411979-fb7e4b44-adca-435e-ae1d-7ee6bde276d8.png)


![image](https://user-images.githubusercontent.com/81233165/139411990-8945a039-b31f-4531-9a45-89e240c8325f.png)


![image](https://user-images.githubusercontent.com/81233165/139412012-e1cc8d34-d20c-4ad7-93a4-a9fdebf94b6d.png)


The file system contains three lists. One list is assigned to the header and the others to the contents.

Header:

File name- the name of the file in the file system.
Size- the size of the file in bytes.
Date- the creation time of the file in the file system.
Link- bool operator which tell if the file is a link file or not.
Hash- md5 hash string, which we use to check that the content match to the header.
index- store the start position of the content in the file system.
len- store the length of the content in the file system.
Existing file- if the link is true, it points to an existing file in the file system, else it is null.

Content:

File- string that contains the content. We represent binary data as string format.
Size- the size of the file in bytes.
Extra size- we use it to know the real size of a cell (node). When we remove a file, there is a hole and we need to know the size of the cell when we want to add a new content or use the optimize function, so if we want to cover the hole we need to check that the new content is not bigger then the size of the cell. For example, if the size of a content is 1000 and it was deleted so there is a hole in size 1000. Lets add a new file with size 700, the extra size will 300 and the size of the content is 700. This way, we don't lose the real size of the cell.
Hash- md5 hash string, which we use to check that the content match to the header.

Contents list :

Each node contains an array from type long which have two cells: the first cell store the start position of the content in the file system, and the second one store the length.

ContentDeleted list :

Store the deleted contents. 
When we remove a file from the file system, we remove it from the contents list, and add it to the contentDeleted list. 
