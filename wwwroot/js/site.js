checked = false;
function checkedAll(frm1) {
    let aa = document.getElementById('check');
    checked = checked ? false : true;
    for (var i = 0; i < aa.elements.length; i++) {
        aa.elements[i].checked = checked;
    }
}


