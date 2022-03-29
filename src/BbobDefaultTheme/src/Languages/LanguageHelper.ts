import Bbob from "../../../Bbob/JSApi/Bbob";

let dt = Bbob.meta.extra.defaultTheme;
dt.language = 'zh-CN';
async function getLanguage() {
    if (dt) {
        if (dt.language) {
            if (dt.language == 'zh-CN') {
                return (await import("./zh-CN")).default;
            }
        }
    }
    return (await import('./en-US')).default;
}
let language = await getLanguage();
export {
    language
}