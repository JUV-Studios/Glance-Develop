#ifndef UNDO_REDO_INCLUDE
#define UNDO_REDO_INCLUDE

#include <list>
#include <iostream>
using namespace std;

class undo_stack{

    list<string> undo_list_buffer;
    list<string> redo_list_buffer;    
    //limit indicates depth of history to remember
    int limit;

public:
    undo_stack(int limit = 10); 

    //push is used to insert a new string in undo_list_buffer
    string push(string input);  

    //get_buffer returns string maintained in undo_list_buffer for display purposes
    string get_buffer();    
    
    //undo and redo
        void undo();
        void redo();
    
    //miscelleanous functions
        //updating limit ie changing depth of history maintained
        string reset_undo_count(int limit); 
        //clearing certain number(by_count) of oldest strings in undo_list_buffer and returning a string output of those
        string clear_undo_buffer(int by_count = -1);  // -1 indicated clear all   
    
    //testing 
    void print();
};
//IMPORTANT -> for all functions, returned string type "" shall be treated as NULL!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

#endif