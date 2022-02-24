let offset = 0;
const api = {
    resetNextLinkInfosOffset(new_offset){
        if (!new_offset){
            new_offset = 0;
        }
        offset = new_offset;
    },
    nextLinkInfos(callback) {
        if (blog.nextFileLinks && offset < blog.nextFileLinks.length) {
            ajaxRequest('get', blog.nextFileLinks[offset], callback)
            offset++
        }
        else{
            callback(false);
        }
    },
    getArticleFromAddress(address, callback) {
        ajaxRequest('get', address, callback);
    },
    getLinkInfosWithTag(tagName, callback) {
        if (blog.tags.length) {
            blog.tags.forEach(tag => {
                if (tag == tagName) {
                    let r = Math.random();
                    ajaxRequest('get', `${meta.publicPath}bbob.assets/tags/${tag}.json?r=${r}`, callback)
                }
            });
        }
        else{
            callback(false)
        }
    },
    getLinkInfosWithCategory(categoryName, callback) {
        if (blog.categories.length) {
            blog.categories.forEach(category => {
                if (category == categoryName) {
                    let r = Math.random();
                    ajaxRequest('get', `${meta.publicPath}bbob.assets/categories/${category}.json?r=${r}`, callback)
                }
            });
        }
        else{
            callback(false)
        }
    },
    getLinkInfosWithArchiveAddress(archiveAddress, callback) {
        ajaxRequest('get', archiveAddress, callback);
    }
}

let ajaxRequest = function(type, url, callback) {
    let xmlHttp = new XMLHttpRequest();
    xmlHttp.open(type, url, true);
    xmlHttp.send(null);
    xmlHttp.onreadystatechange = function () {
        if (xmlHttp.readyState == 4 && xmlHttp.status == 200) {
            callback(JSON.parse(xmlHttp.responseText))
        }
    }
}