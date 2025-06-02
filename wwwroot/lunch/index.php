<?php
header('Content-Type: application/json');

date_default_timezone_set('America/New_York'); // Set timezone to Eastern Time

// Summer lunch data
$summer_lunches = [
    // Week 1
    ["Quesadilla, apples, yogurt chips"],
    ["Ham/salami, cantaloupe, chips, baby carrots"],
    ["Chicken sticks, strawberries, peppers, chips"],
    ["Pizza, oranges/banana, GoGurt"],
    ["Kids pick"],
    // Week 2
    ["Lunchable, blueberries, grapes, cucumbers, chips"],
    ["Peanut butter sandwich, apples, cheese sticks, chips"],
    ["Fish sticks, pineapple, carrots, chips"],
    ["Ham/salami sandwich, oranges, peppers, chips"],
    ["Kids pick"],
    // Week 3
    ["Chicken sticks, blueberries, cucumbers, chips"],
    ["Pizza, oranges, banana, yogurt"],
    ["Quesadilla, apples, yogurt chips"],
    ["Lunchable, blueberry, grapes, chips"],
    ["Kids pick"]
];

// School lunch data
$school_lunches = [
    // Week 1
    ["Chicken Patty Sandwich, Popcorn Chicken Salad, Honey Glazed Carrots"],
    ["Nacho Supreme, Taco Salad, Refried Beans"],
    ["Bosco Sticks, Yogurt Parfait with Homemade Granola, Steamed Broccoli"],
    ["Pasta with Assorted Sauces & Garlic Breadstick, Taco Salad, Green Beans"],
    ["Chicken Nuggets with Blueberry Muffin, Popcorn Chicken Salad, Baked Apples"],
    // Week 2
    ["Soft Pretzel with Cheese, Popcorn Chicken Salad, Green Beans"],
    ["Hot Dog, Taco Salad, Baked Beans"],
    ["Personal Pan Pizza, Yogurt Parfait with Homemade Granola, Steamed Broccoli"],
    ["French Toast Sticks with Sausage Links, Taco Salad, Baked Blueberry Cobbler"],
    ["Chicken Smackers with Dinner Roll, Popcorn Chicken Salad, Mashed Potatoes with Gravy"],
    // Week 3
    ["Orange Chicken Rice Bowl, Popcorn Chicken Salad, Steamed Broccoli"],
    ["Mini Corn Dogs, Taco Salad, Smiley Potatoes"],
    ["Cheeseburger, Yogurt Parfait with Homemade Granola, Baked Beans"],
    ["Pizza Crunchers with Dip, Taco Salad, Green Beans"],
    ["Chicken Tenders with Biscuit, Popcorn Chicken Salad, Buttered Corn"]
];

// Events data
$events = [
    '2025-06-02' => 'Test Event NB',
    '2025-08-04' => 'Teacher Work Day (No Students)',
    '2025-08-05' => 'Teacher Work Day (No Students)',
    '2025-08-06' => 'First Student Day',
    '2025-09-01' => 'Labor Day Holiday',
    '2025-10-06' => 'Fall Break',
    '2025-10-07' => 'Fall Break',
    '2025-10-08' => 'Fall Break',
    '2025-10-09' => 'Fall Break',
    '2025-10-10' => 'Fall Break',
    '2025-11-06' => 'Elem. Parent/Teacher Conf. (Elem. Students Dismissed Half-Day)',
    '2025-11-07' => 'Elem. Parent/Teacher Conf. (No School for Elem. Students)',
    '2025-11-26' => 'Thanksgiving Break',
    '2025-11-27' => 'Thanksgiving Break',
    '2025-11-28' => 'Thanksgiving Break',
    '2025-12-19' => 'Teacher Work Day (No Students)',
    '2025-12-22' => 'Winter Break',
    '2025-12-23' => 'Winter Break',
    '2025-12-24' => 'Winter Break',
    '2025-12-25' => 'Winter Break',
    '2025-12-26' => 'Winter Break',
    '2025-12-29' => 'Winter Break',
    '2025-12-30' => 'Winter Break',
    '2025-12-31' => 'Winter Break',
    '2026-01-01' => 'Winter Break',
    '2026-01-02' => 'Winter Break',
    '2026-01-05' => 'Teacher Work Day (No Students)',
    '2026-01-19' => 'Martin Luther King, Jr. Holiday',
    '2026-02-16' => 'Presidents’ Day Holiday',
    '2026-04-06' => 'Spring Break',
    '2026-04-07' => 'Spring Break',
    '2026-04-08' => 'Spring Break',
    '2026-04-09' => 'Spring Break',
    '2026-04-10' => 'Spring Break',
    '2026-05-22' => 'Last Student Day',
    '2026-05-25' => 'Memorial Day (Schools & Offices Closed)',
    '2026-05-26' => 'Teacher Work Day (No Students)'
];


$start_date_summer = new DateTime('2025-05-23');  // Start date of the summer cycle
$start_date_school = new DateTime('2025-08-06');  // Start date of the school cycle
$end_date_school = new DateTime('2026-05-22');    // End date of the school cycle
$today = new DateTime();                          // Today's date

$current_time = new DateTime();                   // Current time to decide the menu shift

// Check if current time is past 2 PM and adjust the date accordingly
if ($current_time->format('H') >= 14) {
    $today->add(new DateInterval('P1D'));  // Move to the next day if after 2 PM
}

function getWeekdaysCount($from, $to) {
    $is_weekend = function($date) {
        return $date->format('N') >= 6;
    };

    $days_count = 0;
    $current = clone $from;
    while ($current <= $to) {
        if (!$is_weekend($current)) {
            $days_count++;
        }
        $current->add(new DateInterval('P1D'));
    }
    return $days_count;
}

// Determine if we are in the school schedule or summer schedule
if ($today >= $start_date_school && $today <= $end_date_school) {
    // School schedule
    $total_weekdays = getWeekdaysCount($start_date_school, $today) - 1; // Subtract 1 to count from zero
    $cycle_day = $total_weekdays % 15;                                 // 15 weekdays in the cycle, index from 0 to 14
    $menu = $school_lunches[$cycle_day];
} else {
    // Summer schedule
    $total_weekdays = getWeekdaysCount($start_date_summer, $today) - 1; // Subtract 1 to count from zero
    $cycle_day = $total_weekdays % 15;                                  // 15 weekdays in the cycle, index from 0 to 14
    $menu = $summer_lunches[$cycle_day];
}

$response = [
    'date' => $today->format('Y-m-d'),
    'menu' => $menu
];

// Check if there's an event on the current date
$today_str = $today->format('Y-m-d');
if (array_key_exists($today_str, $events)) {
    $response['event'] = $events[$today_str];
}

echo json_encode($response);
?>