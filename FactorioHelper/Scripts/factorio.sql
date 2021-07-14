SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

CREATE DATABASE IF NOT EXISTS `factorio` DEFAULT CHARACTER SET utf8 COLLATE utf8_bin;
USE `factorio`;

CREATE TABLE `build_type` (
  `id` int(10) UNSIGNED NOT NULL,
  `name` varchar(255) COLLATE utf8_bin NOT NULL,
  `description` text COLLATE utf8_bin
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

INSERT INTO `build_type` (`id`, `name`, `description`) VALUES
(1, 'Assembling machine', 'Item built in an assembling machine.'),
(2, 'Chemical plant', 'Item built in a chemical plant with a single recipe (exclude items such as "Solid fuel").'),
(3, 'Furnace', 'Item melted in a furnace.'),
(4, 'Mining drill', 'Item mined with a mining drill.'),
(5, 'Refining', 'Item built in a refinery (or a chemical plant) by following a recipe; several recipes are available to build the item.'),
(6, 'Other', 'Item that doesn\'t fit into other categories (such as "water" or "Steam").');

CREATE TABLE `component` (
  `target_item_id` int(10) UNSIGNED NOT NULL,
  `source_item_id` int(10) UNSIGNED NOT NULL,
  `quantity` int(10) UNSIGNED NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

INSERT INTO `component` (`target_item_id`, `source_item_id`, `quantity`) VALUES
(1, 2, 1),
(1, 3, 1),
(2, 6, 1),
(3, 4, 2),
(4, 5, 1),
(7, 8, 1),
(7, 9, 1),
(8, 3, 1),
(8, 4, 1),
(9, 3, 1),
(9, 4, 1),
(9, 10, 1),
(10, 4, 1),
(10, 11, 3),
(11, 2, 1),
(12, 13, 2),
(12, 14, 1),
(12, 15, 1),
(13, 16, 5),
(14, 4, 5),
(14, 18, 10),
(15, 2, 5),
(15, 19, 1),
(15, 20, 1),
(16, 17, 2),
(19, 4, 4),
(20, 4, 5),
(21, 22, 3),
(21, 23, 2),
(21, 24, 1),
(22, 10, 2),
(22, 11, 4),
(22, 25, 2),
(23, 3, 1),
(23, 20, 1),
(23, 27, 2),
(24, 26, 30),
(24, 44, 30),
(25, 18, 1),
(25, 26, 20),
(27, 4, 1),
(28, 29, 1),
(28, 30, 1),
(28, 31, 30),
(29, 16, 10),
(29, 20, 10),
(29, 22, 5),
(30, 10, 5),
(30, 22, 5),
(31, 17, 1),
(31, 20, 1),
(31, 32, 1),
(32, 4, 1),
(33, 34, 2),
(33, 35, 1),
(33, 36, 3),
(34, 10, 20),
(34, 22, 2),
(34, 37, 5),
(35, 10, 3),
(35, 20, 1),
(35, 38, 2),
(35, 39, 1),
(36, 2, 20),
(36, 20, 2),
(36, 25, 5),
(37, 4, 1),
(37, 24, 5),
(37, 44, 100),
(38, 2, 1),
(38, 4, 1),
(38, 37, 20),
(39, 10, 2),
(39, 23, 1),
(39, 40, 15),
(40, 41, 10),
(46, 36, 10),
(46, 47, 10),
(46, 48, 10),
(47, 34, 1),
(47, 49, 1),
(48, 41, 10),
(48, 50, 10),
(49, 10, 5),
(49, 22, 5),
(50, 42, 10),
(51, 34, 100),
(51, 36, 100),
(51, 48, 50),
(51, 52, 100),
(51, 53, 5),
(51, 54, 100),
(52, 4, 2),
(52, 38, 5),
(53, 3, 5),
(53, 4, 10),
(53, 10, 5),
(54, 2, 5),
(54, 10, 15),
(54, 20, 5),
(55, 46, 100),
(55, 51, 1);

CREATE TABLE `item` (
  `id` int(10) UNSIGNED NOT NULL,
  `name` varchar(255) COLLATE utf8_bin NOT NULL,
  `build_time` decimal(4,1) NOT NULL,
  `build_result` int(10) UNSIGNED NOT NULL DEFAULT '1',
  `build_type_id` int(10) UNSIGNED NOT NULL,
  `is_science_pack` tinyint(1) UNSIGNED NOT NULL DEFAULT '0',
  `apply_real_requirement` tinyint(1) UNSIGNED NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

INSERT INTO `item` (`id`, `name`, `build_time`, `build_result`, `build_type_id`, `is_science_pack`, `apply_real_requirement`) VALUES
(1, 'Automation science pack', '5.0', 1, 1, 1, 0),
(2, 'Copper plate', '3.2', 1, 3, 0, 0),
(3, 'Iron gear wheel', '1.0', 1, 1, 0, 0),
(4, 'Iron plate', '3.2', 1, 3, 0, 0),
(5, 'Iron ore', '1.0', 1, 4, 0, 0),
(6, 'Copper ore', '1.0', 1, 4, 0, 0),
(7, 'Logistic science pack', '6.0', 1, 1, 1, 0),
(8, 'Transport belt', '0.5', 2, 1, 0, 0),
(9, 'Inserter', '0.5', 1, 1, 0, 0),
(10, 'Electronic circuit', '0.5', 1, 1, 0, 0),
(11, 'Copper cable', '0.5', 2, 1, 0, 0),
(12, 'Military science pack', '10.0', 2, 1, 1, 0),
(13, 'Wall', '0.5', 1, 1, 0, 0),
(14, 'Grenade', '8.0', 1, 1, 0, 0),
(15, 'Piercing rounds magazine', '3.0', 1, 1, 0, 0),
(16, 'Stone brick', '3.2', 1, 3, 0, 0),
(17, 'Stone', '1.0', 1, 4, 0, 0),
(18, 'Coal', '1.0', 1, 4, 0, 0),
(19, 'Firearm magazine', '1.0', 1, 1, 0, 0),
(20, 'Steel plate', '16.0', 1, 3, 0, 0),
(21, 'Chemical science pack', '24.0', 2, 1, 1, 0),
(22, 'Advanced circuit', '6.0', 1, 1, 0, 0),
(23, 'Engine unit', '10.0', 1, 1, 0, 0),
(24, 'Sulfur', '1.0', 2, 2, 0, 0),
(25, 'Plastic bar', '1.0', 2, 2, 0, 0),
(26, 'Petroleum gas', '1.0', 1, 5, 0, 0),
(27, 'Pipe', '0.5', 1, 1, 0, 0),
(28, 'Production science pack', '21.0', 3, 1, 1, 0),
(29, 'Electric furnace', '5.0', 1, 1, 0, 0),
(30, 'Productivity module', '15.0', 1, 1, 0, 0),
(31, 'Rail', '0.5', 2, 1, 0, 0),
(32, 'Iron stick', '0.5', 2, 1, 0, 0),
(33, 'Utility science pack', '21.0', 3, 1, 1, 0),
(34, 'Processing unit', '10.0', 1, 1, 0, 0),
(35, 'Flying robot frame', '20.0', 1, 1, 0, 0),
(36, 'Low density structure', '20.0', 1, 1, 0, 0),
(37, 'Sulfuric acid', '1.0', 50, 2, 0, 0),
(38, 'Battery', '4.0', 1, 2, 0, 0),
(39, 'Electric engine unit', '10.0', 1, 1, 0, 0),
(40, 'Lubricant', '1.0', 10, 2, 0, 0),
(41, 'Heavy oil', '1.0', 1, 5, 0, 0),
(42, 'Light oil', '1.0', 1, 5, 0, 0),
(43, 'Crude oil', '1.0', 1, 6, 0, 0),
(44, 'Water', '1.0', 1, 6, 0, 0),
(45, 'Steam', '1.0', 1, 6, 0, 0),
(46, 'Rocket part', '3.0', 1, 1, 0, 1),
(47, 'Rocket control unit', '30.0', 1, 1, 0, 0),
(48, 'Rocket fuel', '30.0', 1, 1, 0, 0),
(49, 'Speed module', '15.0', 1, 1, 0, 0),
(50, 'Solid fuel (light oil)', '2.0', 1, 2, 0, 0),
(51, 'Satellite', '5.0', 1, 1, 0, 1),
(52, 'Accumulator', '10.0', 1, 1, 0, 0),
(53, 'Radar', '0.5', 1, 1, 0, 0),
(54, 'Solar panel', '10.0', 1, 1, 0, 0),
(55, 'Space science pack', '40.3', 1000, 6, 1, 1);

CREATE TABLE `recipe` (
  `id` int(10) UNSIGNED NOT NULL,
  `name` varchar(255) COLLATE utf8_bin NOT NULL,
  `build_type_id` int(10) UNSIGNED NOT NULL DEFAULT '5',
  `build_time` decimal(4,1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

INSERT INTO `recipe` (`id`, `name`, `build_type_id`, `build_time`) VALUES
(1, 'Basic oil processing', 5, '5.0'),
(2, 'Advanced oil processing', 5, '5.0'),
(3, 'Coal liquefaction', 5, '5.0'),
(4, 'Light oil cracking', 2, '2.0'),
(5, 'Heavy oil cracking', 2, '2.0');

CREATE TABLE `recipe_source` (
  `recipe_id` int(10) UNSIGNED NOT NULL,
  `item_id` int(10) UNSIGNED NOT NULL,
  `quantity` int(10) UNSIGNED NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

INSERT INTO `recipe_source` (`recipe_id`, `item_id`, `quantity`) VALUES
(1, 43, 100),
(2, 43, 100),
(2, 44, 50),
(3, 18, 10),
(3, 41, 25),
(3, 45, 50),
(4, 42, 30),
(4, 44, 30),
(5, 41, 40),
(5, 44, 30);

CREATE TABLE `recipe_target` (
  `recipe_id` int(10) UNSIGNED NOT NULL,
  `item_id` int(10) UNSIGNED NOT NULL,
  `quantity` int(10) UNSIGNED NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

INSERT INTO `recipe_target` (`recipe_id`, `item_id`, `quantity`) VALUES
(1, 26, 45),
(2, 26, 55),
(2, 41, 25),
(2, 42, 45),
(3, 26, 10),
(3, 41, 90),
(3, 42, 20),
(4, 26, 20),
(5, 42, 30);


ALTER TABLE `build_type`
  ADD PRIMARY KEY (`id`);

ALTER TABLE `component`
  ADD PRIMARY KEY (`target_item_id`,`source_item_id`),
  ADD KEY `source_item_id` (`target_item_id`),
  ADD KEY `destination_item_id` (`source_item_id`);

ALTER TABLE `item`
  ADD PRIMARY KEY (`id`),
  ADD KEY `build_type_id` (`build_type_id`);

ALTER TABLE `recipe`
  ADD PRIMARY KEY (`id`),
  ADD KEY `build_type_id` (`build_type_id`);

ALTER TABLE `recipe_source`
  ADD PRIMARY KEY (`recipe_id`,`item_id`),
  ADD KEY `recipe_id` (`recipe_id`),
  ADD KEY `item_id` (`item_id`);

ALTER TABLE `recipe_target`
  ADD PRIMARY KEY (`recipe_id`,`item_id`),
  ADD KEY `recipe_id` (`recipe_id`),
  ADD KEY `item_id` (`item_id`);


ALTER TABLE `build_type`
  MODIFY `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;
ALTER TABLE `item`
  MODIFY `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=56;
ALTER TABLE `recipe`
  MODIFY `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

ALTER TABLE `component`
  ADD CONSTRAINT `component_ibfk_1` FOREIGN KEY (`target_item_id`) REFERENCES `item` (`id`),
  ADD CONSTRAINT `component_ibfk_2` FOREIGN KEY (`source_item_id`) REFERENCES `item` (`id`);

ALTER TABLE `item`
  ADD CONSTRAINT `item_ibfk_1` FOREIGN KEY (`build_type_id`) REFERENCES `build_type` (`id`);

ALTER TABLE `recipe`
  ADD CONSTRAINT `recipe_ibfk_1` FOREIGN KEY (`build_type_id`) REFERENCES `build_type` (`id`);

ALTER TABLE `recipe_source`
  ADD CONSTRAINT `recipe_source_ibfk_1` FOREIGN KEY (`recipe_id`) REFERENCES `recipe` (`id`),
  ADD CONSTRAINT `recipe_source_ibfk_2` FOREIGN KEY (`item_id`) REFERENCES `item` (`id`);

ALTER TABLE `recipe_target`
  ADD CONSTRAINT `recipe_target_ibfk_1` FOREIGN KEY (`recipe_id`) REFERENCES `recipe` (`id`),
  ADD CONSTRAINT `recipe_target_ibfk_2` FOREIGN KEY (`item_id`) REFERENCES `item` (`id`);

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
