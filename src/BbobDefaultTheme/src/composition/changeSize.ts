import { ref } from "vue";

let normal = ref(true);
let maxWidth: number | undefined = undefined;
let maxHeight: number | undefined = undefined;
const onResize = () => {
    if ((maxWidth && window.outerWidth <= maxWidth) ||
        (maxHeight && window.outerHeight <= maxHeight)) {
        normal.value = false;
    }
    else {
        normal.value = true;
    }
}
onResize();
window.onresize = onResize;
function setMaxWidth(width: number | undefined) {
    maxWidth = width;
    onResize();
}
function setMaxHeight(height: number | undefined) {
    maxHeight = height;
    onResize();
}
function getMaxWidth() {
    return maxWidth;
}
function getMaxHeight() {
    return maxHeight;
}

export {
    normal,
    setMaxWidth,
    setMaxHeight,
    getMaxWidth,
    getMaxHeight
}