#ifndef MAIN_CONTENT_INCLUDE
#define MAIN_CONTENT_INCLUDE

#include <iostream>
#include "undo_redo.hpp"
using namespace std;

class main_content{
    string *main_content;
    undo_stack *stack_buffer;
    bool use_undo;
public:
    //simple main_content initializer, no undo/redo functionality
    //main_content();  -- not sure if passing nothing in default parameterised constructor conflists with default constructor 

    //initialize main_content with undo/redo functionality, passing limit 
    main_content(bool add_undo_redo = 0,int limit = 10);
    string get();
    void push(string input); 
};

#endif