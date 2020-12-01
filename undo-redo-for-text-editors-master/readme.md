UNDO FOR TEXT EDITOR
    The project implements undo/redo functionality for text editors.

To use, link file undo_redo.o with your code.
Eg - g++ yourcode.cc undo_redo.o -o yourprogram

Class undo_stack in undo_redo.hpp implements the following:
1. undo_stack(int limit) - initialize a undo_stack and mentioning its buffer size
2. string push(string input) -  used to insert a new string in undo buffer, oldest string in undo buffer is returned if buffer becomes full, also redo                                              buffer is cleared
3. string get_buffer() -  returns string maintained in undo buffer for display purposes by text editor
4. void undo() -  removes newest string from undo buffer adding it to redo buffer
5. void redo() -  removes newest string from redo buffer adding it to undo buffer
6. string reset_undo_count(int limit) - updates buffer size to passed parameter
7. string clear_undo_buffer(int by_count) - clearing certain number(by_count) of oldest strings in undo_list_buffer and returning a string output of those, passing                                             a neg number will clear the entire buffer 

For further detials see undo_redo.hpp

main_content.hpp will be developed as a text editor interface using undo_redo.hpp as an example
helloworld.cc was used for testing purposes 
