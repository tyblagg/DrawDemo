import React, { Component } from 'react';

const GameVoting = ({ visible, voters, color, submitVote }) => {

    let createButtons = () => {
        let table = [];

        // Outer loop to create parent
        voters.forEach((v) => {
            if (v.color != color) {
                table.push(<tr><button onClick={() => submitVote(v.connectionId)}>{v.name}</button></tr>);
            }
        });

        return table;
    }

    if (!visible) {
        return <div />
    }
    else {
        return (
            <table>
                {createButtons()}
            </table>
        );
    }
}

export default GameVoting