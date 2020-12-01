#include "main_content.hpp"

main_content::main_content(bool add_undo_redo = 0,int limit = 10)
{
    main_content = new string();
    if(add_undo_redo)
    {
        use_undo = 0;   
    }
    else
    {
        use_undo = 1;
        stack_buffer = new undo_stack(limit);
    }
}


string main_content::get()
{
    cout<<main_content;
}

void main_content::push(string input)
{
    if(use_undo)
    {
        main_content += input;
    }
}

void activate_undo(int limit)
{
    stack_buffer = new undo_stack(limit);
}