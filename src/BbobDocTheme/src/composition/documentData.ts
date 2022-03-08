import { ref } from "vue";
import Bbob from "../../../Bbob/JSApi/Bbob";

let toc = ref('')
let rawHtml = ref('<h1 style="text-align: center;">Welcome to documents!!!</h1>')
let activeName = ref('0')

function getArticle(address: string) {
    if (address == 'default')return;
    let a =address;
    const sa = Bbob.meta.extra.shortAddress;
    if (sa) {
        a = `${sa.startOfAddress}${address}${sa.endOfAddress}`;
    }
    Bbob.api.getArticleFromAddress(a, (article) => {
        if (article.contentParsed) {
            rawHtml.value = article.contentParsed;
        }
        if (article.toc) {
            toc.value = article.toc;
        }
        activeName.value = address;
    })
}

function clearCache(){
    toc.value = '';
    rawHtml.value = '<h1 style="text-align: center;">Welcome to documents!!!</h1>';
    activeName.value = '0'
}

export {
    toc, rawHtml, getArticle, activeName, clearCache
}