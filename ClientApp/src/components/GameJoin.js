import React, { Component } from 'react';

const GameJoin = ({ visible, mySubmitHandler, myChangeHandler}) => {
        
    if (!visible) {
        return <div />
    }
    else {
        return (
            <form onSubmit={mySubmitHandler}>
                <p>Join game with code:</p>
                <input
                    type='text'
                    name='gamecode'
                    onChange={myChangeHandler}
                />
                <p>Enter your name:</p>
                <input
                    type='text'
                    name='username'
                    onChange={myChangeHandler}
                /><br/>
                <input
                    type='submit'
                    value='Join'
                />
            </form>
        );
    }
}

export default GameJoin