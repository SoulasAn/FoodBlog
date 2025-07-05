window.foodBlogEditor = {
    exec: function(command) {
        document.execCommand(command, false, null);
    },
    getHtml: function(id) {
        var el = document.getElementById(id);
        return el ? el.innerHTML : '';
    },
    setHtml: function(id, html) {
        var el = document.getElementById(id);
        if(el) el.innerHTML = html || '';
    }
};
