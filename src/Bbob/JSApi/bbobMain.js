let offset = 0;
const api = {
    resetNextLinkInfosOffset(new_offset) {
        if (!new_offset) {
            new_offset = 0;
        }
        offset = new_offset;
    },
    nextLinkInfos(callback) {
        if (blog.nextFileLinks && offset < blog.nextFileLinks.length) {
            ajaxRequest({ url: blog.nextFileLinks[offset] }).then(callback).catch(callback);
            offset++
        }
        else {
            callback(undefined);
        }
    },
    async nextLinkInfosAsync() {
        if (blog.nextFileLinks && offset < blog.nextFileLinks.length) {
            let r = await ajaxRequest({ url: blog.nextFileLinks[offset] })
            offset++
            return r;
        }
    },
    getArticleFromAddress(address, callback) {
        ajaxRequest({ url: address }).then(callback).catch(callback);
    },
    async getArticleFromAddressAsync(address) {
        return await ajaxRequest({ url: address });
    },
    getLinkInfosWithAddress(address, callback) {
        ajaxRequest({ url: address }).then(callback).catch(callback);
    },
    async getLinkInfosWithAddressAsync(address) {
        return await ajaxRequest({ url: address });
    },
    executeScriptElements(containerElement) {
        const scriptElements = containerElement.querySelectorAll("script");

        Array.from(scriptElements).forEach((scriptElement) => {
            const clonedElement = document.createElement("script");

            Array.from(scriptElement.attributes).forEach((attribute) => {
                clonedElement.setAttribute(attribute.name, attribute.value);
            });

            clonedElement.text = scriptElement.text;

            scriptElement.parentNode.replaceChild(clonedElement, scriptElement);
        });
    }
}

let ajaxRequest = async function (requestObj) {
    return new Promise((resolve, reject) => {
        let xhr = new XMLHttpRequest();
        xhr.open(requestObj.method || "GET", requestObj.url);
        if (requestObj.headers) {
            Object.keys(requestObj.headers).forEach(key => {
                xhr.setRequestHeader(key, requestObj.headers[key]);
            });
        }
        xhr.onreadystatechange = function () {
            if (xhr.readyState != 4) return;
            if (xhr.status == 200) {
                resolve(JSON.parse(xhr.responseText))
            }
            else reject(undefined);
        }
        xhr.onerror = () => reject(xhr.statusText);
        xhr.send(requestObj.body);
    });
}