#include <iostream>
#include "undo_redo.hpp"
using namespace std;

int main()
{   
    cout<<"hello world!\n";
    undo_stack undo(1);
    cout<<undo.push("aba\n");
    cout<<undo.push("cde\n");
    undo.undo();
    undo.print();
    cout<<undo.push("ghi\n");
    undo.print();

    cout<<undo.push("jkl\n");
    cout<<undo.clear_undo_buffer();
    undo.reset_undo_count(1);
    cout<<undo.push("mno\n");
    cout<<undo.push("pqr\n");
    return 0;
}