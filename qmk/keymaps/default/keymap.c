// Copyright 2023 QMK
// SPDX-License-Identifier: GPL-2.0-or-later

#include QMK_KEYBOARD_H

// Define os combos
const uint16_t PROGMEM combo_ae[] = {KC_A, KC_E, COMBO_END};
const uint16_t PROGMEM combo_ai[] = {KC_A, KC_I, COMBO_END};

combo_t key_combos[] = {
    COMBO(combo_ae, KC_X),  // A+E juntos = X
    COMBO(combo_ai, KC_Z),  // A+I juntos = Z
};

const uint16_t PROGMEM keymaps[][MATRIX_ROWS][MATRIX_COLS] = {
    /*
     * ┌───┬───┬───┬───┐
     * │ 7 │ 8 │ 9 │ / │
     * ├───┼───┼───┼───┤
     * │ 4 │ 5 │ 6 │ * │
     * ├───┼───┼───┼───┤
     * │ 1 │ 2 │ 3 │ - │
     * ├───┼───┼───┼───┤
     * │ 0 │ . │Ent│ + │
     * └───┴───┴───┴───┘
     */
    [0] = LAYOUT(
        KC_A,   KC_E,   KC_I,   KC_O
    )
};
