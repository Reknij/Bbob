import Bbob from "../../../Bbob/JSApi/Bbob";

let dt = Bbob.meta.extra.defaultTheme;
async function getLanguage() {
    if (dt) {
        if (dt.language) {
            if (dt.language.startsWith('en')) return (await import('./en-US')).default;
            if (dt.language == 'zh-CN') return (await import('./zh-CN')).default;
            if (dt.language == 'zh-TW') return (await import('./zh-TW')).default;
            if (dt.language.startsWith('zh')) return (await import('./zh-CN')).default;
            if (dt.language.startsWith('ar')) return (await import('./ar')).default;
            if (dt.language.startsWith('ja')) return (await import('./ja-JP')).default;
            if (dt.language.startsWith('ko')) return (await import('./ko-KR')).default;
            if (dt.language.startsWith('ru')) return (await import('./ru-RU')).default;
            if (dt.language.startsWith('fr')) return (await import('./fr-FR')).default;
            if (dt.language.startsWith('es')) return (await import('./es-ES')).default;
            if (dt.language.startsWith('de')) return (await import('./de-DE')).default;
        }
    }
    return (await import('./en-US')).default;
}
let language = await getLanguage();
export {
    language
}