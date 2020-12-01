// a simple plugin to undo/redo text
#include "undo_redo.hpp"

undo_stack::undo_stack(int limit)
{
    if(limit>0)
        this->limit = limit;
    else
        this->limit = 0;
}

string undo_stack::push(string input)
{
    redo_list_buffer.clear();
    if(limit == 0)
        return input;
    string popped_element = "";  //SOLVED -> //ISSUE: handle memory deallocation 
    if(undo_list_buffer.size()>=limit && limit>0)
    {
        popped_element = undo_list_buffer.front();
        undo_list_buffer.pop_front();
    }
    undo_list_buffer.push_back(input);
    return popped_element;
}

string get_buffer()
{
    string res = "";
    for(auto it=undo_list_buffer.begin();it!=undo_list_buffer.end();it++)
        res += *it;
    return res;    
}

void undo_stack::undo()
{
    if(!undo_list_buffer.empty())
    {
        redo_list_buffer.push_back(undo_list_buffer.back());
        undo_list_buffer.pop_back();
    }
}

void undo_stack::redo()
{
    if(!redo_list_buffer.empty())
    {
        undo_list_buffer.push_back(redo_list_buffer.front());
        redo_list_buffer.pop_front();
    }
}

string undo_stack::reset_undo_count(int limit)
{
    redo_list_buffer.clear();  // for consistency purpose
    int old_limit = this->limit;
    if(limit>0)
        this->limit = limit;
    else
        this->limit = 0;
    int diff_bw_new_old_sizes = old_limit - max(0,limit);
    if(diff_bw_new_old_sizes>0)
        return clear_undo_buffer(diff_bw_new_old_sizes);
    return "";
}

string undo_stack::clear_undo_buffer(int by_count)
{
    string res = "";    
    //if by_count is neg empty entire entire buffer
    if(by_count<0)  
    {
        by_count = undo_list_buffer.size(); //update by_count value to entire undo_list_buffer size
    }
    while(!undo_list_buffer.empty() && by_count) //handling by_count size more than number of elements in undo_list_buffer
    {
        res += undo_list_buffer.front();
        undo_list_buffer.pop_front();
        by_count--;
    }
    return res;
}

void undo_stack::print()
{
    cout<<"undo buffer:\n";
    for(auto it=undo_list_buffer.begin();it!=undo_list_buffer.end();it++)
        cout<<*it<<" , ";
    cout<<"redo buffer\n";
    for(auto it=redo_list_buffer.begin();it!=redo_list_buffer.end();it++)
        cout<<*it<<" , ";
    cout<<endl;        
}