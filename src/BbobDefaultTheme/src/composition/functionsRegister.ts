import { ref } from "vue"


let scrollToDownInvoke = ()=>{
    scrollToDownFunctions.value.forEach(func => {
        func();
    });
}
let scrollToDownFunctions = ref<Function[]>([])

function scrollToDownEventRegist(func: Function){
    scrollToDownFunctions.value.push(func);
    return scrollToDownFunctions.value.length - 1;
}

function scrollToDownEventUnRegist(index: number){
    scrollToDownFunctions.value.splice(index, 1);
}

export {
    scrollToDownInvoke,
    scrollToDownEventRegist,
    scrollToDownEventUnRegist
}