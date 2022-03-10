import React from 'react'
import SketchPad from '../store/SketchPad';
import { TOOL_PENCIL } from '../tools'


const GameCanvas = ({ visible, color, items, submitDrawing, onCompleteItem, disabled }) => {
    const tool = TOOL_PENCIL;
    const size = 2;
    const fill = false;
    const fillColor = '#444444';

    if (!visible) {
        return <div />
    }
    else {
        return (
            <div>
                <button onClick={submitDrawing}>Done!</button>
                <SketchPad
                    animate={true}
                    disabled={disabled}
                    size={size}
                    color={color}
                    fillColor={fill ? fillColor : ''}
                    items={items}
                    tool={tool}
                    onCompleteItem={onCompleteItem}
                />
            </div>
        );
    }
}


export default GameCanvas