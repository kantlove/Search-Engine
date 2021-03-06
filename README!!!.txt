+=======================================================================================+
| Information Retrieval									|
| Author: Pham Minh Thai								|
| APCS - HCMUS										|
+=======================================================================================+

████████████████████████████████████  1. WHAT'S NEW  ████████████████████████████████████

- HURRAY! Finally, a new interface has come. 
- Smaller generated file size.
- Better performance.

██████████████████████████████████  2. RUN THE PROGRAM ██████████████████████████████████ 

Easy way:
=========
1 step: download the full project at http://1drv.ms/1NOXqVu

Hard way: please do the following steps
========================================
Step 1: copy all content (or sub-folders) inside 20_newsgroups folder into 
	/bin/Debug/docs/
Step 2: run
Step 3: wait for the program to finish while drinking some tea.
Step 4: look at the graph

-----------------------------------------------------------------------------------------
- Information will be displayed on both Console and the Window
- By default, the program used processed data.
- Parameters (file Parameter.cs):
	+ RESET		: true - calculate everything again (take long time!)
			  false - use previous data
	+ TERMS_LIMIT	: number of maximum terms to be processed in each doc

- Graph will be displayed once the program finished.
- Please do not DELETE anything thing inside bin\Debug !!!
 
- Common issues:
	+ Missing reference	: all needed libraries are included in /packages folder
					please re-reference them.
	+ OutOfBound		: please re-run the app.
	+ NullPointer		: please re-run the app.
	+ Out of memory		: try to decrease DOCS_LIMIT and TERMS_LIMIT.


██████████████████████████████████  3. OUTPUT STRUCTURE ██████████████████████████████████

I changed the doc id to [category index]_[doc name].
	category_index 	= index of a category (0, 1, 2, ...)
	doc_name 	= name of the document file

** The information below is old. You don't need to care about it **
All generated files are stored in bin\Debug\output and bin\Debug\ folder.

A. Queries
	- Queries are generated each run.
	- Method: take random words from many documents, those documents will be the 
	solution.
	- Queries are stored in bin\Debug\queries\queries.txt

B. Terms
	- Terms are extracted from all documents after removing stop words
	- Terms are stored in bin\Debug\output\_TERM_.txt

C. Inverted File
	- Created using SPIMI method
	- Stored in bin\Debug\output\_SPIMI_.txt
	- Format:
		Term  |  PostingList

D. Rank List
	- A rank list is created by sorted the results from comparison of angles between 
	query vector and document vectors
	- Ranklist is stored in bin\Debug\output\result\query_{i}.txt
	(i = query number)
	- Format:
		docId  |  Angle

E. Precision and recall
	- Precision and recall values of each rank list is stored in bin\Debug\output\result
	 \pre_recall_{i}.txt
	(i = query number)
	- Format:
		Precision  |  Recall

F. Graphs
	- Graphs are drawed by the program and stored in 
	bin\Debug\output\result\graph_{i}.xls	(i = query number)
